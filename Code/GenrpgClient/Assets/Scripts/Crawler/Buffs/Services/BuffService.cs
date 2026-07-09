using Assets.Scripts.Crawler.Items.Services;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Buffs.Settings;
using OxDb.SharedGame.Crawler.Combat.Services;
using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.Info.Services;
using OxDb.SharedGame.Crawler.Items.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Roles.Services;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.Spells.Entities;
using OxDb.SharedGame.Crawler.Spells.Services;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Upgrades.Constants;
using OxDb.SharedGame.Stats.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Buffs.Services
{
    public interface IBuffService : IInjectable
    {
        List<PartyBuff> GetMissingBuffs(PartyData party);
        List<Role> RolesThatCanBuff(long partyBuffId);
        string GetMissingBuffsString(PartyData party);
        float GetPartyBuffPower(PartyData party, long partyBuffId);
        Task CastAllPartyBuffs(PartyData party, CancellationToken token);
    }


    public class BuffService : IBuffService
    {

        class BuffCaster
        {
            public PartyMember Member { get; set; }
            public double Power { get; set; }
            public long Mana { get; set; }
            public long MaxMana { get; set; }
            public MemberItemSpell ItemSpell { get; set; }
        }

        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private ICrawlerSpellService _spellService = null;
        private IInfoService _infoService = null;
        private ICrawlerService _crawlerService = null;
        private ICrawlerCombatService _combatService = null;
        private IRoleService _roleService = null;
        private ICrawlerItemService _itemService = null;
        private ICrawlerUpgradeService _upgradeService = null;

        public List<Role> RolesThatCanBuff(long partyBuffId)
        {
            CrawlerSpell spell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData().FirstOrDefault(x => x.Effects.Count == 1 &&
            x.Effects[0].EntityTypeId == EntityTypes.PartyBuff && x.Effects[0].EntityId == partyBuffId);

            if (spell != null)
            {
                return _spellService.RolesThatCanCast(spell.IdKey);
            }
            return new List<Role>();
        }

        public List<PartyBuff> GetMissingBuffs(PartyData party)
        {
            IReadOnlyList<PartyBuff> allBuffs = _gameData.Get<PartyBuffSettings>(_gs.ch).GetData();

            List<long> roleIds = new List<long>();

            foreach (PartyMember member in party.ActiveParty)
            {
                roleIds.AddRange(member.Roles.Select(x => x.RoleId));
            }

            roleIds = roleIds.Distinct().OrderBy(x => x).ToList();

            List<Role> partyRoles = _gameData.Get<RoleSettings>(_gs.ch).GetData().Where(r => roleIds.Contains(r.IdKey)).ToList();

            List<long> partyRoleIds = partyRoles.Select(x => x.IdKey).Distinct().ToList();

            CrawlerSpellSettings spellSettings = new CrawlerSpellSettings();

            List<PartyBuff> missingBuffs = new List<PartyBuff>();

            foreach (PartyBuff partyBuff in allBuffs)
            {
                List<Role> buffRoles = RolesThatCanBuff(partyBuff.IdKey);

                if (!buffRoles.FastAny(r => partyRoleIds.Contains(r.IdKey)))
                {
                    missingBuffs.Add(partyBuff);
                }
            }

            return missingBuffs;
        }

        public string GetMissingBuffsString(PartyData party)
        {
            List<PartyBuff> missingBuffs = GetMissingBuffs(party);

            StringBuilder sb = new StringBuilder();

            sb.Append("Missing Party Buffs: ");

            for (int p = 0; p < missingBuffs.Count; p++)
            {
                sb.Append(_infoService.CreateInfoLink(missingBuffs[p]) + (p < missingBuffs.Count - 1 ? ", " : ""));
            }

            if (missingBuffs.Count == 0)
            {
                sb.Append("None.");
            }

            return sb.ToString();
        }

        public async Task CastAllPartyBuffs(PartyData party, CancellationToken token)
        {
            if (_crawlerService.GetState() != ECrawlerStates.ExploreWorld)
            {
                return;
            }

            IReadOnlyList<PartyBuff> allBuffs = _gameData.Get<PartyBuffSettings>(_gs.ch).GetData();

            Dictionary<PartyMember, List<CrawlerSpell>> spellDict = new Dictionary<PartyMember, List<CrawlerSpell>>();

            List<PartyMember> members = party.ActiveParty;

            members.Reverse();

            foreach (PartyMember member in members)
            {
                if (_combatService.IsDisabled(member))
                {
                    continue;
                }

                spellDict[member] = _spellService.GetAbilitiesForMember(party, member, true);

            }

            IReadOnlyList<CrawlerSpell> allSpells = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData();

            foreach (PartyBuff pbuff in allBuffs)
            {
                CrawlerSpell spell = allSpells.FirstOrDefault(x => x.Effects.Count == 1 && x.Effects[0].EntityTypeId == EntityTypes.PartyBuff &&
                x.Effects[0].EntityId == pbuff.IdKey);

                if (spell == null)
                {
                    continue;
                }

                List<BuffCaster> casters = new List<BuffCaster>();

                BuffCaster currCaster = null;

                foreach (PartyMember member in party.ActiveParty)
                {
                    currCaster = new BuffCaster()
                    {
                        Member = member,
                        Mana = member.Stats.Curr(StatTypes.Mana),
                        MaxMana = member.Stats.Max(StatTypes.Mana)
                    };

                    casters.Add(currCaster);

                    if (spellDict[member].FastAny(x => x.IdKey == spell.IdKey))
                    {
                        long cost = _spellService.GetPowerCost(party, member, spell);

                        if (cost <= currCaster.Mana)
                        {
                            currCaster.Power = _roleService.GetSpellScalingLevel(party, member, spell, false);
                        }
                    }

                    List<MemberItemSpell> itemSpellStart = _itemService.GetUsableItemsForMember(party, member);

                    foreach (MemberItemSpell itemSpell in itemSpellStart)
                    {
                        if (itemSpell.ChargesLeft < 1)
                        {
                            continue;
                        }

                        Effect effect = itemSpell.UsableItem.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.CrawlerSpell && x.EntityId == spell.IdKey);

                        if (effect != null && effect.Quantity >= currCaster.Power)
                        {
                            currCaster.ItemSpell = itemSpell;
                            currCaster.Power = effect.Quantity;
                        }
                    }
                }

                List<BuffCaster> orderedCasters =
                    casters.OrderByDescending(x => x.Power)
                    .ThenByDescending(x => x.ItemSpell != null ? 1 : 0)
                    .ThenByDescending(x => x.Mana).
                    ThenByDescending(x => x.MaxMana).ToList();

                if (orderedCasters.Count > 0 && orderedCasters[0].Power > 0)
                {
                    float newTier = GetPartyBuffPower(party, pbuff.IdKey);

                    if (party.Buffs[pbuff.IdKey] > newTier - 0.001f)
                    {
                        continue;
                    }

                    UnitAction action = _combatService.GetActionFromSpell(party, orderedCasters[0].Member, spell, null, orderedCasters[0].ItemSpell?.UsableItem ?? null);

                    await _spellService.CastSpell(party, action, token);
                    await Awaitable.NextFrameAsync(token);
                }
            }

            await Task.CompletedTask;
        }

        public float GetPartyBuffPower(PartyData party, long partyBuffId)
        {
            if (party.ActiveParty.Count < 1)
            {
                return 1;
            }

            long maxLevel = party.ActiveParty.Max(x => x.Level);

            float baseValue = (float)Math.Sqrt(1 + maxLevel);

            baseValue *= (float)(1 + _upgradeService.GetPartyBonus(party, PartyUpgrades.PartyBuffPower));

            return baseValue;
        }
    }
}



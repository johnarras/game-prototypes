using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.Items.Entities;
using OxDb.SharedGame.Crawler.Items.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Spells.Services;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.Upgrades.Constants;
using OxDb.SharedGame.Inventory.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Crawler.Items.Services
{
    public interface ICrawlerItemService : IInjectable
    {
        long GetItemUsesPerCombat(PartyData party);
        List<MemberItemSpell> GetUsableItemsForMember(PartyData party, PartyMember member);
    }

    public class CrawlerItemService : ICrawlerItemService
    {

        private ICrawlerSpellService _spellService = null;
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private ICrawlerUpgradeService _upgradeService = null;


        public long GetItemUsesPerCombat(PartyData party)
        {
            return (long)(_gameData.Get<CrawlerItemSettings>(_gs.ch).MaxUsesBetweenCombats +
                _upgradeService.GetPartyBonus(party, PartyUpgrades.ItemUsesPerCombat, 0));
        }
        public List<MemberItemSpell> GetUsableItemsForMember(PartyData party, PartyMember member)
        {

            long maxUses = GetItemUsesPerCombat(party);

            CrawlerSpellSettings spellSettings = _gameData.Get<CrawlerSpellSettings>(_gs.ch);

            List<MemberItemSpell> memberList = new List<MemberItemSpell>();
            foreach (Item item in member.Equipment)
            {
                Effect spellEffect = item.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.CrawlerSpell);
                if (spellEffect == null)
                {
                    continue;
                }
                CrawlerSpell spell = spellSettings.Get(spellEffect.EntityId);

                if (spell == null)
                {
                    continue;
                }

                long chargesLeft = Math.Max(0, maxUses - party.ItemsUsed.Count(x => x == item.Id));

                if (party.Combat != null)
                {
                    if (_spellService.IsNonCombatTarget(spell.TargetTypeId))
                    {
                        continue;
                    }
                }
                else
                {
                    if (_spellService.IsEnemyTarget(spell.TargetTypeId))
                    {
                        continue;
                    }
                }

                MemberItemSpell args = new MemberItemSpell()
                {
                    Member = member,
                    UsableItem = item,
                    Spell = spell,
                    ChargesLeft = chargesLeft,
                };

                memberList.Add(args);
            }

            return memberList;
        }
    }
}



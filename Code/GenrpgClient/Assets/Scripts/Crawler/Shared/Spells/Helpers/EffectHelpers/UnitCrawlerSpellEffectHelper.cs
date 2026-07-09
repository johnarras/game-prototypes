using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Combat.Constants;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Spells.Entities;
using OxDb.SharedGame.Units.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public class UnitCrawlerSpellEffectHelper : BaseCrawlerSpellEffectHelper
    {
        public override long HelperKey => EntityTypes.Unit;

        public override async ValueTask ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token)
        {


            PartyMember partyMember = caster as PartyMember;
            long unitTypeId = fullEffect.Effect.EntityId;

            if (partyMember == null && unitTypeId == 0)
            {
                unitTypeId = caster.UnitTypeId;
            }

            UnitType unitType = _gameData.Get<UnitTypeSettings>(null).Get(unitTypeId);

            if (unitType == null)
            {
                args.FullAction = $"{caster.Name} tries to summon an unknown ally.";
                return;
            }

            if (party.Combat != null)
            {
                long quantity = RandUtils.LongRange(fullEffect.Hit.MinQuantity, fullEffect.Hit.MaxQuantity, _gs.Rand);

                SummonArgs summonArgs = null;

                if (caster is PartyMember member)
                {
                    quantity = 1;

                    summonArgs = new SummonArgs()
                    {
                        SummonTier = _roleService.GetSpellScalingLevel(party, member, spell.Spell, true),
                        SummonStatBonus = (long)(_crawlerStatService.GetStatBonus(party, member, spell.StatScalingTypeId) *
                        _gameData.Get<CrawlerSpellSettings>(_gs.ch).SummonStatBonusScale),
                    };
                }

                InitialCombatGroup icg = new InitialCombatGroup()
                {
                    Quantity = quantity,
                    UnitTypeId = unitTypeId,
                    FactionTypeId = caster.FactionTypeId,
                    Level = caster.Level,
                    Range = CrawlerCombatConstants.MinRange,
                    SummonArgs = summonArgs,
                };

                _combatService.AddCombatUnits(party, icg);

                await Task.CompletedTask;
            }
            else if (partyMember != null)
            {
                long currRoleId = -1;

                RoleSettings roleSettings = _gameData.Get<RoleSettings>(null);
                List<Role> playerRoles = roleSettings.GetRoles(partyMember.Roles);
                foreach (Role role in playerRoles)
                {
                    if (role.BinaryBonuses.FastAny(x => x.EntityTypeId == EntityTypes.CrawlerSpell && x.EntityId == spell.Spell.IdKey))
                    {
                        partyMember.Summons = partyMember.Summons.Where(x => x.RoleId != role.IdKey).ToList();

                        currRoleId = role.IdKey;
                    }
                }

                partyMember.Summons.Add(new PartySummon()
                {
                    Name = unitType.Name,
                    UnitTypeId = unitType.IdKey,
                    RoleId = currRoleId,
                });
            }
        }
    }
}


using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public class UnitCrawlerSpellEffectHelper : BaseCrawlerSpellEffectHelper
    {
        public override long HelperKey => EntityTypes.Unit;

        public override async Awaitable ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token)
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
                long quantity = RandUtils.LongRange(fullEffect.Hit.MinQuantity, fullEffect.Hit.MaxQuantity, _rand);

                if (caster is PartyMember member)
                {
                    quantity = _spellService.GetSummonQuantity(party, member, unitType);
                }

                InitialCombatGroup icg = new InitialCombatGroup()
                {
                    Quantity = quantity,
                    UnitTypeId = unitTypeId,
                    FactionTypeId = caster.FactionTypeId,
                    Level = caster.Level,
                    Range = CrawlerCombatConstants.MinRange,
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


using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Spells.Entities;
using OxDb.SharedGame.UnitEffects.Constants;
using OxDb.SharedGame.Units.Settings;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public class PolymorphCrawlerSpellEffectHelper : BaseCrawlerSpellEffectHelper
    {
        public override long HelperKey => EntityTypes.Polymorph;

        public override async Awaitable ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token)
        {

            if (party.Combat == null || target.StatusEffects.HasBitIndex(StatusEffects.Dead))
            {
                return;
            }

            CombatGroup group = party.Combat.GetGroup(target.CombatGroupId);

            if (group == null)
            {
                return;
            }

            UnitType utype = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(fullEffect.Effect.EntityId);

            if (utype == null)
            {
                return;
            }

            group.Units.Remove(target);

            target.StatusEffects.SetBitIndex(StatusEffects.Dead);

            party.Combat.AttackSequence.Remove(target);

            target.CombatActions.Clear();
            InitialCombatGroup icg = new InitialCombatGroup()
            {
                Quantity = 1,
                UnitTypeId = utype.IdKey,
                FactionTypeId = target.FactionTypeId,
                Level = target.Level,
                Range = group.Range,
            };

            _combatService.AddCombatUnits(party, icg);


            await Task.CompletedTask;
        }
    }
}



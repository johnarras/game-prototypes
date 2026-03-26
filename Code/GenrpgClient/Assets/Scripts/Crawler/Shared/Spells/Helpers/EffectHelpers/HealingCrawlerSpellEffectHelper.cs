using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public class HealingCrawlerSpellEffectHelper : BaseCrawlerSpellEffectHelper
    {
        public override long HelperKey => EntityTypes.Healing;

        public override async Awaitable ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token)
        {

            long currHealth = target.Stats.Curr(StatTypes.Health);
            long maxHealth = target.Stats.Max(StatTypes.Health);

            long maxHealing = maxHealth - currHealth;

            if (maxHealing < 1)
            {
                return;
            }
            args.NewQuantity += RandUtils.LongRange(fullEffect.Hit.MinQuantity, fullEffect.Hit.MaxQuantity, _rand);

            long weakReductionPercent = _combatService.GetWeakReductionPercent(caster, spell.Spell.CombatActionId);

            if (weakReductionPercent > 0)
            {
                args.NewQuantity = Math.Max(1, args.NewQuantity * (100 - weakReductionPercent) / 100);
            }

            if (args.NewQuantity > maxHealing)
            {
                args.NewQuantity = maxHealing;
            }
            maxHealing -= args.NewQuantity;

            args.TotalHealing += args.NewQuantity;

            CrawlerUnit finalTarget = target;
            if (args.IsEnemyTarget)
            {
                finalTarget = caster;
            }

            _crawlerStatService.Add(party, finalTarget, StatTypes.Health, UnitStatValOffsets.Curr, args.TotalHealing);
            _spellService.AddToActionDict(args.ActionList, caster, target, "Heals", args.NewQuantity, 0, false, ECombatTextTypes.Healing, 0);


            await Task.CompletedTask;
        }
    }
}



using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.UnitEffects.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public class StatusEffectCrawlerSpellEffectHelper : BaseCrawlerSpellEffectHelper
    {
        public override long HelperKey => EntityTypes.StatusEffect;

        public override async Awaitable ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token)
        {
            IReadOnlyList<StatusEffect> allEffects = _gameData.Get<StatusEffectSettings>(null).GetData();

            if (args.CurrHitTimes > 0)
            {
                return;
            }


            if (fullEffect.Effect.MaxQuantity < 0)
            {
                CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(null);

                double quantityFraction = 1 + Math.Abs(fullEffect.Effect.MaxQuantity * combatSettings.ExtraCureStatusEffectsRemovedPerTier);

                int finalQuantity = (int)quantityFraction;
                if (_rand.NextDouble() < (quantityFraction - finalQuantity))
                {
                    finalQuantity++;
                }

                for (int i = 0; i < allEffects.Count && finalQuantity > 0; i++)
                {

                    if (allEffects[i].IdKey < 1)
                    {
                        continue;
                    }
                    finalQuantity--;
                    if (target.StatusEffects.HasBit(allEffects[i].IdKey))
                    {
                        target.RemoveStatusBit(fullEffect.Effect.EntityId);
                        args.FullAction = $"{caster.Name} Cleanses {target.Name} of {allEffects[i].Name}";
                    }
                }

            }
            else
            {
                StatusEffect statusEffect = allEffects.FirstOrDefault(x => x.IdKey == fullEffect.Effect.EntityId);
                if (fullEffect.Effect != null)
                {

                    if (target.IsPlayer() && _partyService.HasPartyBuff(party, EntityTypes.StatusEffect, fullEffect.Effect.EntityId))
                    {
                        _spellService.ShowCombatLogText($"The party is immune to {fullEffect.Effect.Name}!");
                    }
                    else
                    {
                        IDisplayEffect currentEffect = target.Effects.FirstOrDefault(x =>
                            x.EntityTypeId == EntityTypes.StatusEffect &&
                            x.EntityId == fullEffect.Effect.EntityId);
                        if (currentEffect != null)
                        {
                            if (currentEffect.MaxDuration > 0)
                            {
                                if (fullEffect.Hit.MaxQuantity > currentEffect.MaxDuration)
                                {
                                    currentEffect.MaxDuration = fullEffect.Effect.MaxQuantity;
                                }
                                if (fullEffect.Hit.MaxQuantity > currentEffect.DurationLeft)
                                {
                                    currentEffect.DurationLeft = fullEffect.Effect.MaxQuantity;
                                }
                            }
                        }
                        else
                        {
                            DisplayEffect displayEffect = new DisplayEffect()
                            {
                                MaxDuration = fullEffect.Effect.MaxQuantity,
                                DurationLeft = fullEffect.Effect.MaxQuantity, // MaxQuantity == 0 means infinite
                                EntityTypeId = EntityTypes.StatusEffect,
                                EntityId = fullEffect.Effect.EntityId,
                            };
                            target.AddEffect(displayEffect);
                            args.FullAction = $"{target.Name} is affected by {statusEffect.Name}";
                        }
                    }
                }
            }


            await Task.CompletedTask;
        }
    }
}
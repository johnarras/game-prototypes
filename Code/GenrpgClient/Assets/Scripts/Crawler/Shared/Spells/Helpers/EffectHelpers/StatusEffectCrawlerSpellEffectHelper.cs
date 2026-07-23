using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Combat.Settings;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Spells.Entities;
using OxDb.SharedGame.Spells.Interfaces;
using OxDb.SharedGame.Spells.Settings.Effects;
using OxDb.SharedGame.UnitEffects.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public class StatusEffectCrawlerSpellEffectHelper : BaseCrawlerSpellEffectHelper
    {
        public override long HelperKey => EntityTypes.StatusEffect;

        public override async ValueTask ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token)
        {
            IReadOnlyList<StatusEffect> allEffects = _gameData.Get<StatusEffectSettings>(null).GetData();

            if (args.CurrHitTimes > 0)
            {
                return;
            }


            if (fullEffect.Effect.WeaponDamageScale < 0)
            {
                CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(null);

                double quantityFraction = 1 + Math.Abs(1 * combatSettings.ExtraCureStatusEffectsRemovedPerTier);

                int finalQuantity = (int)quantityFraction;
                if (_gs.Rand.NextDouble() < (quantityFraction - finalQuantity))
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
                    if (target.StatusEffects.HasBitIndex(allEffects[i].IdKey))
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
                                    currentEffect.MaxDuration = (int)fullEffect.Effect.WeaponDamageScale / 10;
                                }
                                if (fullEffect.Hit.MaxQuantity > currentEffect.DurationLeft)
                                {
                                    currentEffect.DurationLeft = (int)fullEffect.Effect.StatBonusDamageScale / 10;
                                }
                            }
                        }
                        else
                        {
                            DisplayEffect displayEffect = new DisplayEffect()
                            {
                                MaxDuration = (int)fullEffect.Effect.WeaponDamageScale / 10,
                                DurationLeft = (int)fullEffect.Effect.WeaponDamageScale / 10, // MaxQuantity == 0 means infinite
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


using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Buffs.Constants;
using OxDb.SharedGame.Crawler.Combat.Constants;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Combat.Settings;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Spells.Services;
using OxDb.SharedGame.Factions.Constants;
using OxDb.SharedGame.Spells.Constants;
using OxDb.SharedGame.Spells.Entities;
using OxDb.SharedGame.Spells.Settings.Effects;
using OxDb.SharedGame.Spells.Settings.Elements;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.UnitEffects.Constants;
using OxDb.SharedGame.UnitEffects.Settings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public abstract class BaseDamageCrawlerSpellEffectHelper : BaseCrawlerSpellEffectHelper
    {
        public override async ValueTask ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token)
        {
            if (target.StatusEffects.HasBitIndex(StatusEffects.Dead))
            {
                return;
            }

            double damageScale = 1.0f;
            long elementBits = (long)(1 << (int)fullEffect.Effect.ElementTypeId);

            ElementType etype = _gameData.Get<ElementTypeSettings>(_gs.ch).Get(fullEffect.Effect.ElementTypeId);
            double finalCritChance = fullEffect.Hit.CritChance;

            bool casterIgnoreResists = caster.FactionTypeId == FactionTypes.Player &&
                _partyService.HasPartyBuff(party, EntityTypes.Element, fullEffect.Effect.ElementTypeId);


            bool targetHasResist = !casterIgnoreResists &&
                (FlagUtils.MatchesAnyBits(target.ResistBits, elementBits) ||
                (target.FactionTypeId == FactionTypes.Player &&
                _partyService.HasPartyBuff(party, EntityTypes.Resist, fullEffect.Effect.ElementTypeId)));


            if (targetHasResist)
            {
                if (!FlagUtils.MatchesAnyBits(target.VulnBits, elementBits))
                {
                    damageScale *= etype.ResistDamagePercent / 100.0;
                    finalCritChance += etype.ResistCritPercentMod;
                    args.ExtraMessageBits |= ExtraMessageBits.Resists;
                }
            }
            else if (FlagUtils.MatchesAnyBits(target.VulnBits, elementBits))
            {
                damageScale *= etype.VulnDamagePercent / 100.0;
                args.ExtraMessageBits |= ExtraMessageBits.Vulnerable;
                finalCritChance += etype.VulnCritPercentMod;
            }

            // Don't allow full crit chance per hit, too strong.
            finalCritChance *= args.CritChanceScaling;

            long weakReductionPercent = _combatService.GetWeakReductionPercent(caster, spell.Spell.CombatActionId);

            if (!target.IsPlayer() && target.DefendRank == 0 && finalCritChance > 0 &&
                _gs.Rand.NextDouble() * 100 < finalCritChance && weakReductionPercent == 0)
            {
                args.NewQuantity = target.Stats.Curr(StatTypes.Health);
                _spellService.AddToActionDict(args.ActionList, caster, target, "CRITS!", args.NewQuantity, args.ExtraMessageBits, false, ECombatTextTypes.Damage, spell.Effects[0].ElementType.IdKey);
                args.DidKill = true;
            }
            else
            {
                CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(null);

                long defenseStatId = StatTypes.Armor;
                args.NewQuantity = RandUtils.LongRange(fullEffect.Hit.MinQuantity, fullEffect.Hit.MaxQuantity, _gs.Rand);
                if (fullEffect.Effect.EntityTypeId == EntityTypes.Damage)
                {
                    defenseStatId = StatTypes.Resist;
                }

                if (weakReductionPercent > 0)
                {
                    args.NewQuantity = Math.Max(1, args.NewQuantity * (100 - weakReductionPercent) / 100);
                }

                if (target.DefendRank == EDefendRanks.Defend)
                {
                    damageScale = combatSettings.DefendDamageScale;
                }
                else if (target.DefendRank == EDefendRanks.Guardian)
                {
                    damageScale = combatSettings.GuardianDamageScale;
                }
                else if (target.DefendRank == EDefendRanks.Taunt)
                {
                    damageScale *= combatSettings.TauntDamageScale;
                }

                args.NewQuantity = (long)Math.Max(1, args.NewQuantity * damageScale);

                long defenseStat = target.Stats.Max(defenseStatId);

                long casterHit = caster.Stats.Max(StatTypes.Hit);
                float defenseStatRatio = 1.0f * casterHit / Math.Max(1, defenseStat);

                double hitChance = defenseStatRatio / combatSettings.GuaranteedHitDefenseRatio;

                bool didMiss = false;
                if (_gs.Rand.NextDouble() > hitChance)
                {
                    _spellService.AddToActionDict(args.ActionList, caster, target, "Misses", 0, ExtraMessageBits.Misses, false, ECombatTextTypes.None, 0);
                    didMiss = true;
                    args.NewQuantity = 0;
                }

                if (casterHit < defenseStat && !didMiss)
                {
                    double ratio = MathUtil.Clamp(combatSettings.MinHitToDefenseRatio
                        , 1.0 * casterHit / defenseStat,
                        combatSettings.MaxHitToDefenseRatio);

                    double newQuantityFract = ratio * args.NewQuantity;

                    args.NewQuantity = (long)newQuantityFract;

                    newQuantityFract -= args.NewQuantity;

                    if (_gs.Rand.NextDouble() < newQuantityFract)
                    {
                        args.NewQuantity++;
                    }

                    args.NewQuantity = Math.Max(1, args.NewQuantity);
                }

                double barrierValue = party.Buffs[PartyBuffs.Barrier];
                if (target.FactionTypeId == FactionTypes.Player && _gs.Rand.NextDouble() * 100 < barrierValue * args.BuffSettings.GetProcChanceScale(PartyBuffs.Barrier))
                {
                    long removedQuantity = Math.Min(args.NewQuantity, (long)(args.NewQuantity * barrierValue * args.BuffSettings.GetEffectScale(PartyBuffs.Barrier) / 100.0));
                    args.NewQuantity -= removedQuantity;

                    if (removedQuantity > 0)
                    {
                        _spellService.AddToActionDict(args.ActionList, target, target, "Absorbed", removedQuantity, 0, false, ECombatTextTypes.Defense, ElementTypes.Earth);
                    }
                }

                if (args.NewQuantity > 0)
                {
                    string actionWord = (fullEffect.Effect.EntityTypeId == EntityTypes.Attack ? "Attacks" :
                        fullEffect.Effect.EntityTypeId == EntityTypes.Shoot ? "Shoots" :
                            fullEffect.ElementType.ObserverActionName);
                    _spellService.AddToActionDict(args.ActionList, caster, target, actionWord, args.NewQuantity, args.ExtraMessageBits, true, ECombatTextTypes.Damage, spell.Effects[0].ElementType.IdKey);
                }
            }

            args.TotalDamage += args.NewQuantity;
            _crawlerStatService.Add(party, target, StatTypes.Health, UnitStatValOffsets.Curr, -args.NewQuantity, fullEffect.Effect.ElementTypeId);


            double cursedArrowsValue = party.Buffs[PartyBuffs.CursedArrows];
            // Sharpshooter do some extra damage.
            if (args.CurrHitTimes == 0 && args.NewQuantity > 0 && fullEffect.Effect.EntityTypeId == EntityTypes.Shoot && caster.IsPlayer() &&
                _gs.Rand.NextDouble() < cursedArrowsValue * args.BuffSettings.GetProcChanceScale(PartyBuffs.CursedArrows))
            {

                long effectTier = (long)(1 + _gs.Rand.NextDouble() * (cursedArrowsValue * cursedArrowsValue * args.BuffSettings.GetEffectScale(PartyBuffs.CursedArrows)));

                StatusEffect statusEffect = _gameData.Get<StatusEffectSettings>(_gs.ch).Get(effectTier);

                if (statusEffect != null && statusEffect.IdKey < StatusEffects.Dead)
                {

                    DisplayEffect displayEffect = new DisplayEffect()
                    {
                        MaxDuration = (int)fullEffect.Effect.WeaponDamageScale,
                        DurationLeft = (int)fullEffect.Effect.WeaponDamageScale, // MaxQuantity == 0 means infinite
                        EntityTypeId = EntityTypes.StatusEffect,
                        EntityId = fullEffect.Effect.EntityId,
                    };
                    target.AddEffect(displayEffect);
                    args.FullAction = $"Accurate Hit! {target.Name} is affected by {statusEffect.Name}";
                }

            }


            await Task.CompletedTask;
        }
    }
}



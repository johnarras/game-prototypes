using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;
using Assets.Scripts.Crawler.ClientEvents.CombatEvents;
using Assets.Scripts.Crawler.ClientEvents.WorldPanelEvents;
using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Inventory.Settings.Ranks;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Spells.Procs.Entities;
using Genrpg.Shared.Spells.Procs.Interfaces;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Spells.Settings.Targets;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Genrpg.Shared.Crawler.Spells.Services
{
    public interface ICrawlerSpellService : IInjectable
    {
        List<CrawlerSpell> GetSpellsForMember(PartyData party, PartyMember member);
        List<CrawlerSpell> GetNonSpellCombatActionsForMember(PartyData party, PartyMember member);
        FullSpell GetFullSpell(PartyData party, CrawlerUnit unit, CrawlerSpell spell, long overrideLevel = 0);
        Task CastSpell(PartyData party, UnitAction action, long overrideLevel, int depth, CancellationToken token);
        ISpecialMagicHelper GetSpecialEffectHelper(long effectEntityId);
        void RemoveSpellPowerCost(PartyData party, CrawlerUnit member, CrawlerSpell spell);
        void SetupCombatData(PartyData party, PartyMember member);
        long GetPowerCost(PartyData party, CrawlerUnit unit, CrawlerSpell spell);
        bool IsEnemyTarget(long targetTypeId);
        bool IsNonCombatTarget(long targetTypeId);
        long GetSummonQuantity(PartyData party, PartyMember member, UnitType unitType);
        void PickRandomTarget(PartyData party, UnitAction unitAction);
    }



    public class CrawlerSpellService : ICrawlerSpellService
    {

        class ExtraMessageBits
        {
            public const long Resists = (1 << 0);
            public const long Vulnerable = (1 << 1);
            public const long Misses = (1 << 2);
        }


        private ILogService _logService = null;
        private ICrawlerCombatService _combatService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IClientRandom _rand = null;
        protected ICrawlerStatService _crawlerStatService = null;
        private ITextService _textService = null;
        private IRoleService _roleService;
        private IDispatcher _dispatcher;
        private ICrawlerService _crawlerService;

        private SetupDictionaryContainer<long, ISpecialMagicHelper> _effectHelpers = new SetupDictionaryContainer<long, ISpecialMagicHelper>();

        public ISpecialMagicHelper GetSpecialEffectHelper(long specialEffectId)
        {
            if (_effectHelpers.TryGetValue(specialEffectId, out ISpecialMagicHelper specialEffectHelper))
            {
                return specialEffectHelper;
            }
            return null;
        }

        public List<CrawlerSpell> GetNonSpellCombatActionsForMember(
            PartyData party, PartyMember member)
        {
            return GetAbilitiesForMember(party, member, false);
        }

        public List<CrawlerSpell> GetSpellsForMember(PartyData party,
            PartyMember member)
        {
            return GetAbilitiesForMember(party, member, true);
        }

        private List<CrawlerSpell> GetAbilitiesForMember(PartyData party,
            PartyMember member, bool chooseSpells)
        {
            EActionCategories actionCategory = party.GetActionCategory();

            IReadOnlyList<CrawlerSpell> allSpells = _gameData.Get<CrawlerSpellSettings>(null).GetData();

            List<CrawlerSpell> castSpells = allSpells.Where(x =>
            (x.CombatActionId == CombatActions.Cast) == chooseSpells).ToList();

            List<CrawlerSpell> okSpells = new List<CrawlerSpell>();

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            Dictionary<long, long> roleScalingTiers = new Dictionary<long, long>();

            IReadOnlyList<RoleScalingType> roleScalingTypes = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).GetData();

            foreach (RoleScalingType roleScaling in roleScalingTypes)
            {
                roleScalingTiers[roleScaling.IdKey] = (long)_roleService.GetRoleScalingLevel(party, member, roleScaling.IdKey);
            }

            if (_combatService.IsDisabled(member))
            {
                return okSpells;
            }

            foreach (CrawlerSpell spell in castSpells)
            {
                if (spell.IdKey < 1)
                {
                    continue;
                }

                if (!roleScalingTiers.ContainsKey(spell.RoleScalingTypeId))
                {
                    _logService.Info("Bad RoleScalingType on " + spell.Name + ": " + spell.RoleScalingTypeId);
                    continue;
                }

                if (spell.RoleScalingTier > roleScalingTiers[spell.RoleScalingTypeId])
                {
                    continue;
                }

                if (_combatService.IsActionBlocked(party, member, spell.CombatActionId))
                {
                    continue;
                }

                if (!roleSettings.HasBonus(member.Roles, EntityTypes.CrawlerSpell, spell.IdKey))
                {
                    continue;
                }

                if (actionCategory == EActionCategories.NonCombat)
                {
                    if (IsEnemyTarget(spell.TargetTypeId))
                    {
                        continue;
                    }

                    // No stat buffs outside of combat for the moment to keep it simpler
                    if (spell.Effects.Any(x => x.EntityTypeId == EntityTypes.Stat))
                    {
                        continue;
                    }
                }
                else // in combat
                {
                    if (IsNonCombatTarget(spell.TargetTypeId))
                    {
                        continue;
                    }

                    // Only defensive things during preparation round.
                    if (actionCategory == EActionCategories.Preparing &&
                        IsEnemyTarget(spell.TargetTypeId))
                    {
                        continue;
                    }

                }
                okSpells.Add(spell);
            }

            List<CrawlerSpell> dupeList = new List<CrawlerSpell>(okSpells);

            foreach (CrawlerSpell dupeSpell in dupeList)
            {
                if (dupeSpell.ReplacesCrawlerSpellId > 0)
                {
                    CrawlerSpell removeSpell = okSpells.FirstOrDefault(x => x.IdKey == dupeSpell.ReplacesCrawlerSpellId);

                    if (removeSpell != null)
                    {
                        okSpells.Remove(removeSpell);
                    }
                }
            }

            okSpells = okSpells.OrderBy(x => x.Name).ToList();

            if (!chooseSpells)
            {
                okSpells = okSpells.OrderBy(x => x.CombatActionId).ThenBy(x => x.TargetTypeId).ToList();
            }
            return okSpells;
        }

        // Figure out what this unit's combat hit will look like.
        public FullSpell GetFullSpell(PartyData party, CrawlerUnit caster, CrawlerSpell spell, long overrideLevel = 0)
        {
            FullSpell fullSpell = new FullSpell() { Spell = spell };

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(null);

            RoleScalingType scalingType = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).Get(spell.RoleScalingTypeId);

            TargetType targetType = _gameData.Get<TargetTypeSettings>(_gs.ch).Get(spell.TargetTypeId);

            double critChance = 0;

            double attackQuantity = 0;

            if (caster is PartyMember member)
            {
                critChance += _gameData.Get<RoleSettings>(_gs.ch).GetRoles(member.Roles).Sum(x => x.CritPercent);
                if (spell.TargetTypeId == TargetTypes.Enemy && member.HideExtraRange > 0)
                {
                    critChance += combatSettings.HiddenSingleTargetCritPercent;
                }
                if (spell.CombatActionId != CombatActions.Hide)
                {
                    member.HideExtraRange = 0;
                }
                critChance += spell.ExtraCritChance;

                if (party.Combat != null)
                {
                    member.LastCombatCrawlerSpellId = spell.IdKey;
                }
            }

            CombatAction action = _gameData.Get<CombatActionSettings>(_gs.ch).Get(spell.CombatActionId);

            List<long> actionTypesWithProcsSet = new List<long>();

            ElementTypeSettings elemSettings = _gameData.Get<ElementTypeSettings>(null);

            // Make full effect list to let us weave procs into the combined spell's effects.
            List<FullEffect> startFullEffectList = new List<FullEffect>();

            foreach (CrawlerSpellEffect effect in spell.Effects)
            {
                startFullEffectList.Add(new FullEffect() { Effect = effect, Chance = 1.0, InitialEffect = true });
            }

            List<FullEffect> endFullEffectList = new List<FullEffect>();

            foreach (FullEffect fullEffect in startFullEffectList)
            {
                endFullEffectList.Add(fullEffect);

                ElementType etype = elemSettings.Get(fullEffect.Effect.ElementTypeId);

                if (etype != null && etype.Procs != null)
                {
                    foreach (SpellProc proc in etype.Procs)
                    {
                        endFullEffectList.Add(CreateFullEffectFromProc(proc));
                    }
                }

                if (actionTypesWithProcsSet.Contains(fullEffect.Effect.EntityTypeId))
                {
                    continue;
                }

                actionTypesWithProcsSet.Add(fullEffect.Effect.EntityTypeId);

                List<IProc> procList = GetProcsFromSlot(caster, scalingType.ScalingEquipSlotId);

                foreach (IProc proc in procList)
                {
                    endFullEffectList.Add(CreateFullEffectFromProc(proc));
                }
            }

            Monster monster = caster as Monster;

            if (monster != null && IsEnemyTarget(spell.TargetTypeId))
            {
                endFullEffectList.AddRange(monster.ApplyEffects);
            }

            long statUsedForScaling = scalingType.ScalingStatTypeId;
            foreach (FullEffect fullEffect in endFullEffectList)
            {
                CrawlerSpellEffect effect = fullEffect.Effect;
                ElementType elemType = elemSettings.Get(effect.ElementTypeId);
                if (elemType == null)
                {
                    elemType = elemSettings.Get(ElementTypes.Physical);
                }
                OneEffect oneEffect = new OneEffect();

                fullEffect.Hit = oneEffect;
                fullEffect.ElementType = elemType;
                fullSpell.Effects.Add(fullEffect);

                oneEffect.MinQuantity = CrawlerCombatConstants.BaseMinDamage;
                oneEffect.MaxQuantity = CrawlerCombatConstants.BaseMaxDamage;

                long equipSlotToCheck = scalingType.ScalingEquipSlotId;

                bool finalQuantityIsNegativeAttackCount = false;


                if (effect.EntityTypeId == EntityTypes.Attack)
                {
                    oneEffect.HitType = EHitTypes.Melee;
                }
                else if (effect.EntityTypeId == EntityTypes.Shoot)
                {
                    oneEffect.HitType = EHitTypes.Ranged;
                }
                else
                {
                    oneEffect.HitType = EHitTypes.Spell;
                    if (effect.EntityTypeId == EntityTypes.StatusEffect && effect.MaxQuantity < 0)
                    {
                        finalQuantityIsNegativeAttackCount = true;
                    }
                }

                if (fullEffect.InitialEffect)
                {

                    long luck = caster.Stats.Max(StatTypes.Luck);

                    double luckRatio = luck * 1.0 / caster.Level;

                    luckRatio = Math.Min(luckRatio, combatSettings.MaxLuckCritRatio);

                    critChance += luckRatio * combatSettings.LuckCritChanceAtLevel;

                    oneEffect.CritChance = (long)critChance;
                }

                if (action.QuantityIsBaseAmount)
                {
                    oneEffect.MinQuantity = effect.MinQuantity;
                    oneEffect.MaxQuantity = effect.MaxQuantity;
                }
                else
                {
                    oneEffect.MinQuantity = 0;
                    oneEffect.MaxQuantity = 0;
                }

                Item weapon = caster.GetEquipmentInSlot(equipSlotToCheck);
                if (weapon != null)
                {
                    ItemType itype = _gameData.Get<ItemTypeSettings>(null).Get(weapon.ItemTypeId);
                    LootRank lootRank = _gameData.Get<LootRankSettings>(null).Get(weapon.LootRankId);

                    double minVal = itype.MinVal;
                    double maxVal = itype.MaxVal;

                    if (lootRank != null)
                    {
                        minVal += lootRank.Damage;
                        maxVal += lootRank.Damage;
                    }

                    minVal *= action.WeaponDamageScale;
                    maxVal *= action.WeaponDamageScale;

                    oneEffect.MinQuantity += (long)(minVal);
                    oneEffect.MaxQuantity += (long)(maxVal);

                }
                else if (effect.EntityTypeId == EntityTypes.Attack && monster != null)
                {
                    oneEffect.MinQuantity = monster.MinDam;
                    oneEffect.MaxQuantity = monster.MaxDam;
                }

                double statBonus = _crawlerStatService.GetStatBonus(party, caster, statUsedForScaling) * targetType.StatBonusScale *
                    (spell.StatBonusScaling > 0 ? spell.StatBonusScaling : 1);
                oneEffect.MinQuantity += (long)(Math.Floor(action.StatBonusDamageScale * statBonus));
                oneEffect.MaxQuantity += (long)Math.Ceiling(action.StatBonusDamageScale * statBonus);

                long baseDamageBonus = _crawlerStatService.GetStatBonus(party, caster, StatTypes.DamagePower);

                oneEffect.MinQuantity += baseDamageBonus;
                oneEffect.MaxQuantity += baseDamageBonus;

                oneEffect.MinQuantity = Math.Max(oneEffect.MinQuantity, CrawlerCombatConstants.BaseMinDamage);
                oneEffect.MaxQuantity = Math.Max(oneEffect.MaxQuantity, CrawlerCombatConstants.BaseMaxDamage);

                if (fullEffect.InitialEffect)
                {
                    if (effect.MinQuantity > 0 && effect.MaxQuantity > 0 && !action.QuantityIsBaseAmount)
                    {
                        attackQuantity = MathUtils.LongRange(effect.MinQuantity, effect.MaxQuantity, _rand);
                    }
                    else
                    {
                        double currAttackQuantity = _roleService.GetSpellScalingLevel(party, caster, spell);

                        if (currAttackQuantity > attackQuantity)
                        {
                            attackQuantity = currAttackQuantity;
                        }
                    }
                    // Used for cures.
                    if (finalQuantityIsNegativeAttackCount)
                    {
                        effect.MinQuantity = -(long)attackQuantity;
                        effect.MaxQuantity = -(long)attackQuantity;
                    }
                }
            }

            long intAttackQuantity = (long)(attackQuantity);
            if (_rand.NextDouble() < (attackQuantity - (long)attackQuantity))
            {
                attackQuantity++;
            }

            long luckBonus = _crawlerStatService.GetStatBonus(party, caster, StatTypes.Luck);

            long luckyAttackCount = 0;
            for (int a = 0; a < attackQuantity; a++)
            {
                if (_rand.NextDouble() * 100 < luckBonus)
                {
                    luckyAttackCount++;
                }
            }
            attackQuantity += luckyAttackCount;

            fullSpell.HitQuantity = Math.Max(1, (long)attackQuantity);
            fullSpell.LuckyHitQuantity = luckyAttackCount;
            fullSpell.HitsLeft = fullSpell.HitQuantity;
            return fullSpell;
        }

        private List<IProc> GetProcsFromSlot(CrawlerUnit member, long equipSlotId)
        {
            Item item = member.GetEquipmentInSlot(equipSlotId);

            if (item == null || item.Procs == null || item.Procs.Count < 1)
            {
                return new List<IProc>();
            }

            return new List<IProc>(item.Procs);
        }

        private FullEffect CreateFullEffectFromProc(IProc proc)
        {

            CrawlerSpellEffect procEffect = new CrawlerSpellEffect()
            {
                EntityTypeId = proc.EntityTypeId,
                EntityId = proc.EntityId,
                ElementTypeId = proc.ElementTypeId,
                MinQuantity = proc.MinQuantity,
                MaxQuantity = proc.MaxQuantity,
            };
            FullEffect fullProcEffect = new FullEffect()
            {
                Chance = proc.Chance,
                Effect = procEffect,
            };
            return fullProcEffect;
        }

        public void RemoveSpellPowerCost(PartyData party, CrawlerUnit member, CrawlerSpell spell)
        {

            long powerCost = GetPowerCost(party, member, spell);

            if (powerCost > 0)
            {
                long currMana = member.Stats.Curr(StatTypes.Mana);
                _crawlerStatService.Add(party, member, StatTypes.Mana, StatCategories.Curr, -Math.Min(powerCost, currMana));
            }
        }

        public long GetPowerCost(PartyData party, CrawlerUnit unit, CrawlerSpell spell)
        {
            long tier = (long)_roleService.GetSpellScalingLevel(party, unit, spell);

            return (long)(spell.PowerCost + ((tier) * spell.PowerPerLevel));
        }

        public async Task CastSpell(PartyData party, UnitAction action, long overrideLevel, int depth, CancellationToken token)
        {
            try
            {
                int informationDisplayFrames = 0;

                action.IsComplete = true;
                if (action.Spell == null)
                {
                    return;
                }

                if (_combatService.IsDisabled(action.Caster))
                {
                    if (!action.Caster.StatusEffects.HasBit(StatusEffects.Dead))
                    {
                        await ShowText(party, $"{action.Caster.Name} is disabled!", informationDisplayFrames, false, token);
                    }
                    return;
                }

                if (_combatService.IsActionBlocked(party, action.Caster, action.Spell.CombatActionId))
                {
                    await ShowText(party, $"{action.Caster.Name} was blocked from performing that action!", informationDisplayFrames, false, token);
                    return;
                }

                if (_combatService.ProccedStatusEffect(action.Caster, StatusEffects.Berserk))
                {
                    if (action.Caster.IsPlayer())
                    {
                        await ShowText(party, $"{action.Caster.Name} is BERSERK!", informationDisplayFrames, false, token);
                        List<CrawlerSpell> allSpells = GetAbilitiesForMember(party, action.Caster as PartyMember, true);

                        List<CrawlerSpell> possibleSpells = new List<CrawlerSpell>();

                        long mana = action.Caster.Stats.Curr(StatTypes.Mana);
                        foreach (CrawlerSpell spell in allSpells)
                        {
                            if (GetPowerCost(party, action.Caster, spell) <= mana)
                            {
                                possibleSpells.Add(spell);
                            }
                        }

                        if (possibleSpells.Count > 0)
                        {
                            CrawlerSpell newSpell = possibleSpells[_rand.Next(possibleSpells.Count)];

                            UnitAction newUnitAction = _combatService.GetActionFromSpell(party, action.Caster, newSpell);

                            PickRandomTarget(party, newUnitAction);

                            if (newUnitAction.FinalTargets.Count < 1)
                            {
                                await ShowText(party, $"{action.Caster.Name} could not find a target for {newSpell.Name}!", informationDisplayFrames, false, token);
                                return;
                            }
                            action = newUnitAction;
                        }
                    }
                }

                if (_combatService.ProccedStatusEffect(action.Caster, StatusEffects.Clumsy))
                {
                    await ShowText(party, $"{action.Caster.Name} is Clumsy and fails to do anything!", informationDisplayFrames, false, token);
                    return;
                }

                if (_combatService.ProccedStatusEffect(action.Caster, StatusEffects.Confused))
                {
                    await ShowText(party, $"{action.Caster.Name} is Confused and targets the wrong thing!", informationDisplayFrames, false, token);

                    UnitAction newUnitAction = _combatService.GetActionFromSpell(party, action.Caster, action.Spell);

                    PickRandomTarget(party, newUnitAction);

                    return;
                }

                FullSpell fullSpell = GetFullSpell(party, action.Caster, action.Spell, overrideLevel);

                bool foundOkTarget = false;
                if (!IsNonCombatTarget(fullSpell.Spell.TargetTypeId))
                {
                    foreach (CrawlerUnit unit in action.FinalTargets)
                    {
                        if (unit.StatusEffects.HasBit(StatusEffects.Dead))
                        {
                            continue;
                        }
                        foundOkTarget = true;
                        break;
                    }
                }
                else
                {
                    foundOkTarget = true;
                }

                if (!foundOkTarget)
                {
                    return;
                }

                if (!fullSpell.Spell.HasFlag(CrawlerSpellFlags.SuppressCastText) && fullSpell.LuckyHitQuantity < 1)
                {
                    await ShowText(party, $"{action.Caster.Name} casts {fullSpell.Spell.Name}", informationDisplayFrames, false, token);
                    if (fullSpell.LuckyHitQuantity == 1)
                    {
                        await ShowText(party, _textService.HighlightText("1 Lucky Hit!", TextColors.ColorGold), informationDisplayFrames, false, token);
                    }
                    else if (fullSpell.LuckyHitQuantity > 1)
                    {
                        await ShowText(party, _textService.HighlightText($"{fullSpell.LuckyHitQuantity} Lucky Hits!", TextColors.ColorGold), informationDisplayFrames, false, token);
                    }
                }

                if (action.Caster is PartyMember pmember)
                {
                    RemoveSpellPowerCost(party, pmember, action.Spell);
                }

                if (party.Combat != null)
                {
                    if (!string.IsNullOrEmpty(action.Caster.PortraitName))
                    {
                        _dispatcher.Dispatch(new SetWorldPicture(action.Caster.PortraitName, false));
                    }

                    if (action.FinalTargets.Count == 0 || action.FinalTargets[0].DefendRank < EDefendRanks.Guardian)
                    {
                        List<CombatGroup> groups = new List<CombatGroup>();

                        if (action.Spell.TargetTypeId == TargetTypes.AllEnemies)
                        {
                            groups = action.Caster.FactionTypeId == FactionTypes.Player ? party.Combat.Enemies : party.Combat.Allies;
                        }
                        else if (action.Spell.TargetTypeId == TargetTypes.OneEnemyGroup)
                        {
                            groups = action.FinalTargetGroups;
                        }
                        else if (action.Spell.TargetTypeId == TargetTypes.AllAllies)
                        {
                            groups = action.Caster.FactionTypeId == FactionTypes.Player ? party.Combat.Allies : party.Combat.Enemies;
                        }

                        if (groups.Count > 0)
                        {
                            groups = groups.Where(x => x.Range >= action.Spell.MinRange && x.Range <= action.Spell.MaxRange).ToList();

                            action.FinalTargets = new List<CrawlerUnit>();

                            foreach (CombatGroup group in groups)
                            {
                                action.FinalTargets.AddRange(group.Units);
                            }
                        }
                    }
                }


                if (action.FinalTargets.Count > 0)
                {
                    long originalHitsLeft = fullSpell.HitsLeft;
                    string combatGroupId = action.FinalTargets[0].CombatGroupId;
                    foreach (CrawlerUnit target in action.FinalTargets)
                    {
                        if (fullSpell.Spell.TargetTypeId == TargetTypes.EnemyInEachGroup &&
                            target.CombatGroupId != combatGroupId)
                        {
                            fullSpell.HitsLeft = originalHitsLeft;
                            combatGroupId = target.CombatGroupId;
                        }

                        await CastSpellOnUnit(party, action.Caster, fullSpell, target, CrawlerCombatConstants.GetScrollingFrames(party.ScrollFramesIndex), token);
                    }
                }
            }
            catch (Exception e)
            {
                _logService.Exception(e, "CastSpell");
            }
        }

        private async Task ShowText(PartyData party, string text, int delayFrames, bool updateGroups, CancellationToken token)
        {

            _dispatcher.Dispatch(new AddActionPanelText(text));

            if (delayFrames > 0 && !_crawlerService.TriggerSpeedupNow())
            {
                int framesComplete = 0;
                while (++framesComplete < delayFrames)
                {
                    await Awaitable.NextFrameAsync(token);
                    if (_crawlerService.TriggerSpeedupNow())
                    {
                        break;
                    }
                }
            }
        }

        private void AddToActionDict(Dictionary<string, ActionListItem> dict, string actionName, long quantity, long extraMessageBits, bool regularHit, ECombatTextTypes textType, long elementTypeId)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                return;
            }

            if (!dict.ContainsKey(actionName))
            {
                dict[actionName] = new ActionListItem();
            }

            if (dict[actionName].ElementTypeId == 0)
            {
                dict[actionName].ElementTypeId = elementTypeId;
            }
            dict[actionName].TotalQuantity += quantity;
            dict[actionName].TotalHits++;
            dict[actionName].ExtraMessageBits |= extraMessageBits;
            dict[actionName].IsRegularHit = regularHit;
            dict[actionName].TextType = textType;

        }

        internal class ActionListItem
        {
            public long ElementTypeId { get; set; }
            public long TotalQuantity { get; set; }
            public long TotalHits { get; set; }
            public long ExtraMessageBits { get; set; }
            public bool IsRegularHit { get; set; }
            public ECombatTextTypes TextType { get; set; }
        }

        public async Task CastSpellOnUnit(PartyData party, CrawlerUnit caster, FullSpell spell, CrawlerUnit target, int delayFrames, CancellationToken token)
        {
            if (spell.Spell.TargetTypeId == TargetTypes.OneEnemyGroup ||
                spell.Spell.TargetTypeId == TargetTypes.AllAllies ||
                spell.Spell.TargetTypeId == TargetTypes.AllEnemies)
            {
                spell.HitsLeft = Math.Max(spell.HitQuantity, 1);
            }
            if (caster.StatusEffects.HasBit(StatusEffects.Cursed))
            {
                spell.HitsLeft = Math.Max(1, spell.HitsLeft / 2);
            }

            bool isEnemyTarget = IsEnemyTarget(spell.Spell.TargetTypeId);

            if (isEnemyTarget && target.StatusEffects.HasBit(StatusEffects.Dead))
            {
                return;
            }

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(null);
            RoleSettings roleSettings = _gameData.Get<RoleSettings>(null);

            long currHealth = target.Stats.Curr(StatTypes.Health);
            long maxHealth = target.Stats.Max(StatTypes.Health);

            long maxDamage = currHealth;
            long maxHealing = maxHealth - currHealth;

            bool haveMultiHitEffect = spell.Effects.Any(x =>
            x.Effect.EntityTypeId == EntityTypes.Damage ||
            x.Effect.EntityTypeId == EntityTypes.Healing ||
            x.Effect.EntityTypeId == EntityTypes.Attack ||
            x.Effect.EntityTypeId == EntityTypes.Shoot);

            if (!haveMultiHitEffect)
            {
                spell.HitsLeft = 1;
            }

            long totalDamage = 0;
            long totalHealing = 0;

            int currHitTimes = 0;
            long newQuantity = 0;
            string fullAction = null;

            long casterHit = caster.Stats.Max(StatTypes.Hit);
            CrawlerUnit finalTarget = target;

            long weakReductionPercent = _combatService.GetWeakReductionPercent(caster, spell.Spell.CombatActionId);

            Dictionary<string, ActionListItem> actionList = new Dictionary<string, ActionListItem>();

            long extraMessageBits = 0;

            double critChanceScaling = 1.0f;
            while (spell.HitsLeft > 0)
            {
                bool didKill = false;
                foreach (FullEffect fullEffect in spell.Effects)
                {
                    if (_rand.NextDouble() > fullEffect.Chance)
                    {
                        continue;
                    }

                    if (didKill)
                    {
                        break;
                    }

                    newQuantity = 0;
                    fullAction = null;
                    finalTarget = target;
                    extraMessageBits = 0;
                    CrawlerSpellEffect effect = fullEffect.Effect;
                    OneEffect hit = fullEffect.Hit;

                    long finalMinQuantity = hit.MinQuantity;
                    long finalMaxQuantity = hit.MaxQuantity;

                    if (effect.EntityTypeId == EntityTypes.Attack ||
                        effect.EntityTypeId == EntityTypes.Shoot ||
                        effect.EntityTypeId == EntityTypes.Damage)
                    {
                        if (target.StatusEffects.HasBit(StatusEffects.Dead))
                        {
                            continue;
                        }

                        double damageScale = 1.0f;
                        long elementBits = (long)(1 << (int)effect.ElementTypeId);

                        double finalCritChance = hit.CritChance;
                        if (FlagUtils.IsSet(target.ResistBits, elementBits))
                        {
                            if (!FlagUtils.IsSet(target.VulnBits, elementBits))
                            {
                                damageScale /= combatSettings.VulnerabilityDamageMult;
                                finalCritChance += combatSettings.ResistAddCritChance;
                                extraMessageBits |= ExtraMessageBits.Resists;
                            }
                        }
                        else if (FlagUtils.IsSet(target.VulnBits, elementBits))
                        {
                            damageScale *= combatSettings.VulnerabilityDamageMult;
                            extraMessageBits |= ExtraMessageBits.Vulnerable;
                            finalCritChance += combatSettings.VulnAddCritChance;
                        }

                        // Don't allow full crit chance per hit, too strong.
                        finalCritChance *= critChanceScaling;

                        if (!target.IsPlayer() && target.DefendRank == 0 && finalCritChance > 0 &&
                            _rand.NextDouble() * 100 < finalCritChance && weakReductionPercent == 0)
                        {
                            newQuantity = target.Stats.Curr(StatTypes.Health);
                            AddToActionDict(actionList, "CRITS!", newQuantity, extraMessageBits, false, ECombatTextTypes.Damage, spell.Effects[0].ElementType.IdKey);
                            didKill = true;
                        }
                        else
                        {
                            long defenseStatId = StatTypes.Armor;
                            newQuantity = MathUtils.LongRange(finalMinQuantity, finalMaxQuantity, _rand);
                            if (effect.EntityTypeId == EntityTypes.Damage)
                            {
                                defenseStatId = StatTypes.Resist;
                            }

                            if (weakReductionPercent > 0)
                            {
                                newQuantity = Math.Max(1, newQuantity * (100 - weakReductionPercent) / 100);
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

                            newQuantity = (long)Math.Max(1, newQuantity * damageScale);

                            long defenseStat = target.Stats.Max(defenseStatId);

                            float defenseStatRatio = 1.0f * casterHit / Math.Max(1, defenseStat);

                            double hitChance = defenseStatRatio / combatSettings.GuaranteedHitDefenseRatio;

                            bool didMiss = false;
                            if (_rand.NextDouble() > hitChance)
                            {
                                AddToActionDict(actionList, "Misses", 0, ExtraMessageBits.Misses, false, ECombatTextTypes.None, 0);
                                didMiss = true;
                                newQuantity = 0;
                            }

                            if (casterHit < defenseStat && !didMiss)
                            {
                                double ratio = MathUtils.Clamp(combatSettings.MinHitToDefenseRatio
                                    , 1.0 * casterHit / defenseStat,
                                    combatSettings.MaxHitToDefenseRatio);

                                double newQuantityFract = ratio * newQuantity;

                                newQuantity = (long)newQuantityFract;

                                newQuantityFract -= newQuantity;

                                if (_rand.NextDouble() < newQuantityFract)
                                {
                                    newQuantity++;
                                }

                                newQuantity = Math.Max(1, newQuantity);
                            }

                            string actionWord = (effect.EntityTypeId == EntityTypes.Attack ? "Attacks" :
                                effect.EntityTypeId == EntityTypes.Shoot ? "Shoots" :
                                    fullEffect.ElementType.ObserverActionName);
                            AddToActionDict(actionList, actionWord, newQuantity, extraMessageBits, true, ECombatTextTypes.Damage, spell.Effects[0].ElementType.IdKey);
                        }
                        totalDamage += newQuantity;
                        _crawlerStatService.Add(party, target, StatTypes.Health, StatCategories.Curr, -totalDamage, effect.ElementTypeId);
                    }
                    else if (effect.EntityTypeId == EntityTypes.Unit)
                    {

                        PartyMember partyMember = caster as PartyMember;
                        long unitTypeId = effect.EntityId;

                        if (partyMember == null && unitTypeId == 0)
                        {
                            unitTypeId = caster.UnitTypeId;
                        }

                        UnitType unitType = _gameData.Get<UnitTypeSettings>(null).Get(unitTypeId);

                        if (unitType == null)
                        {
                            fullAction = $"{caster.Name} tries to summon an unknown ally.";
                            continue;
                        }

                        if (party.Combat != null)
                        {
                            long quantity = MathUtils.LongRange(finalMinQuantity, finalMaxQuantity, _rand);

                            if (caster is PartyMember member)
                            {
                                quantity = GetSummonQuantity(party, member, unitType);
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
                        }
                        else if (partyMember != null)
                        {
                            long currRoleId = -1;
                            foreach (Role role in roleSettings.GetData())
                            {
                                if (role.BinaryBonuses.Any(x => x.EntityTypeId == EntityTypes.CrawlerSpell && x.EntityId == spell.Spell.IdKey))
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
                    else if (effect.EntityTypeId == EntityTypes.Healing)
                    {
                        if (maxHealing < 1)
                        {
                            break;
                        }
                        newQuantity += MathUtils.LongRange(finalMinQuantity, finalMaxQuantity, _rand);

                        if (weakReductionPercent > 0)
                        {
                            newQuantity = Math.Max(1, newQuantity * (100 - weakReductionPercent) / 100);
                        }

                        if (newQuantity > maxHealing)
                        {
                            newQuantity = maxHealing;
                        }
                        maxHealing -= newQuantity;

                        totalHealing += newQuantity;

                        finalTarget = target;
                        if (isEnemyTarget)
                        {
                            finalTarget = caster;
                        }
                        _crawlerStatService.Add(party, target, StatTypes.Health, StatCategories.Curr, totalHealing);
                        AddToActionDict(actionList, "Heals", newQuantity, 0, false, ECombatTextTypes.Healing, 0);
                    }
                    else if (effect.EntityTypeId == EntityTypes.PartyBuff)
                    {
                        double tier = _roleService.GetRoleScalingLevel(party, caster, RoleScalingTypes.Utility);

                        party.Buffs.Set(effect.EntityId, (float)Math.Sqrt(tier));
                        _dispatcher.Dispatch(new UpdateCrawlerUI());
                        _dispatcher.Dispatch(new ShowPartyMinimap() { Party = party });
                    }
                    else if (currHitTimes == 0)
                    {
                        if (effect.EntityTypeId == EntityTypes.StatusEffect)
                        {
                            IReadOnlyList<StatusEffect> allEffects = _gameData.Get<StatusEffectSettings>(null).GetData();

                            if (effect.MaxQuantity < 0)
                            {
                                double quantityFraction = 1 + Math.Abs(effect.MaxQuantity * combatSettings.ExtraCureStatusEffectsRemovedPerTier);

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
                                        target.RemoveStatusBit(effect.EntityId);
                                        fullAction = $"{caster.Name} Cleanses {target.Name} of {allEffects[i].Name}";
                                    }
                                }

                            }
                            else
                            {
                                StatusEffect statusEffect = allEffects.FirstOrDefault(x => x.IdKey == effect.EntityId);
                                if (effect != null)
                                {
                                    IDisplayEffect currentEffect = target.Effects.FirstOrDefault(x =>
                                    x.EntityTypeId == EntityTypes.StatusEffect &&
                                    x.EntityId == effect.EntityId);
                                    if (currentEffect != null)
                                    {
                                        if (currentEffect.MaxDuration > 0)
                                        {
                                            if (hit.MaxQuantity > currentEffect.MaxDuration)
                                            {
                                                currentEffect.MaxDuration = effect.MaxQuantity;
                                            }
                                            if (hit.MaxQuantity > currentEffect.DurationLeft)
                                            {
                                                currentEffect.DurationLeft = effect.MaxQuantity;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        DisplayEffect displayEffect = new DisplayEffect()
                                        {
                                            MaxDuration = effect.MaxQuantity,
                                            DurationLeft = effect.MaxQuantity, // MaxQuantity == 0 means infinite
                                            EntityTypeId = EntityTypes.StatusEffect,
                                            EntityId = effect.EntityId,
                                        };
                                        target.AddEffect(displayEffect);
                                        fullAction = $"{target.Name} is affected by {statusEffect.Name}";
                                    }
                                }
                            }
                        }
                        else if (currHitTimes == 0 && effect.EntityTypeId == EntityTypes.Stat && effect.MaxQuantity > 0)
                        {
                            if (party.Combat != null)
                            {
                                StatVal statVal = party.Combat.StatBuffs.FirstOrDefault(x => x.StatTypeId == effect.EntityId);
                                if (statVal == null)
                                {
                                    statVal = new StatVal()
                                    {
                                        StatTypeId = (short)effect.EntityId,
                                    };
                                    party.Combat.StatBuffs.Add(statVal);
                                }

                                if (statVal.Val < caster.Level)
                                {
                                    statVal.Val = (int)caster.Level;
                                    _crawlerStatService.CalcPartyStats(party, false);
                                }
                            }
                        }

                    }
                    if (!string.IsNullOrEmpty(fullAction))
                    {
                        await ShowText(party, fullAction, delayFrames, false, token);
                    }
                }
                currHitTimes++;
                spell.HitsLeft--;
                critChanceScaling *= combatSettings.CritScaledownPerHit;

                bool isDead = target.Stats.Get(StatTypes.Health, StatCategories.Curr) <= 0;
                if (spell.HitsLeft < 1 || isDead)
                {
                    bool didShowMisses = false;
                    foreach (string actionName in actionList.Keys)
                    {
                        ActionListItem actionListItem = actionList[actionName];

                        string extraWords = "";

                        if (FlagUtils.IsSet(actionListItem.ExtraMessageBits, ExtraMessageBits.Misses))
                        {
                            continue;
                        }

                        if (FlagUtils.IsSet(actionListItem.ExtraMessageBits, ExtraMessageBits.Resists))
                        {
                            extraWords = "(Resist)";
                        }
                        else if (FlagUtils.IsSet(actionListItem.ExtraMessageBits, ExtraMessageBits.Vulnerable))
                        {
                            extraWords = "(Vulnerable)";
                        }

                        string hitText = actionListItem.TotalHits + "x";

                        if (actionListItem.IsRegularHit && !didShowMisses)
                        {
                            long missCount = 0;
                            foreach (ActionListItem item in actionList.Values)
                            {
                                if (FlagUtils.IsSet(item.ExtraMessageBits, ExtraMessageBits.Misses))
                                {
                                    missCount += item.TotalHits;
                                }
                            }

                            if (missCount > 0)
                            {
                                hitText += " (" + missCount + " miss)";
                            }
                            didShowMisses = true;
                        }

                        await ShowText(party, $"{caster.Name} {actionName} {finalTarget.Name} {hitText}"
                            + (actionListItem.TotalQuantity > 0 ? $" for {actionListItem.TotalQuantity} " : "")
                            + " " + $"{extraWords}", delayFrames, false, token);
                        if (actionListItem.TextType != ECombatTextTypes.None && actionListItem.TotalQuantity != 0)
                        {
                            ShowBolt(caster, finalTarget, actionListItem.ElementTypeId);
                            ShowFloatingCombatText(caster, finalTarget,
                                (actionListItem.TextType == ECombatTextTypes.Damage ? "-" : "") + actionListItem.TotalQuantity,
                                actionListItem.TextType, actionListItem.ElementTypeId);
                        }
                    }

                    if (isDead)
                    {
                        target.StatusEffects.SetBit(StatusEffects.Dead);
                        _dispatcher.Dispatch(new UpdateCombatGroups());
                        await ShowText(party, $"{target.Name} is DEAD!\n", 0, true, token);
                        ShowFloatingCombatText(caster, target, "DEAD!", ECombatTextTypes.Info, 0);
                    }
                    break;
                }
            }

            await Task.CompletedTask;
        }


        private void ShowFloatingCombatText(CrawlerUnit caster, CrawlerUnit target, string text, ECombatTextTypes textType, long elementTypeId)
        {
            _dispatcher.Dispatch(new ShowCombatText()
            {
                CasterUnitId = caster.Id,
                CasterGroupId = caster.CombatGroupId,
                TargetGroupId = target.CombatGroupId,
                TargetUnitId = target.Id,
                Text = text,
                TextType = textType,
                ElementTypeId = elementTypeId
            });
        }

        private void ShowBolt(CrawlerUnit caster, CrawlerUnit target, long elementTypeId)
        {
            if (caster != null && target != null && caster != target)
            {
                _dispatcher.Dispatch(new ShowCombatBolt()
                {
                    CasterId = caster.IsPlayer() ? caster.Id : caster.CombatGroupId,
                    TargetId = target.IsPlayer() ? target.Id : target.CombatGroupId,
                    ElementTypeId = elementTypeId,
                    Seconds = 0.1f,
                });
            }
        }

        public void SetupCombatData(PartyData party, PartyMember member)
        {
        }

        public bool IsEnemyTarget(long targetTypeId)
        {
            return targetTypeId == TargetTypes.Enemy ||
                targetTypeId == TargetTypes.OneEnemyGroup ||
                targetTypeId == TargetTypes.AllEnemies ||
                targetTypeId == TargetTypes.EnemyInEachGroup;
        }

        public bool IsNonCombatTarget(long targetTypeId)
        {
            return targetTypeId == TargetTypes.Item ||
                targetTypeId == TargetTypes.Special ||
                targetTypeId == TargetTypes.World;
        }

        public long GetSummonQuantity(PartyData party, PartyMember member, UnitType unitType)
        {
            CrawlerSpell summonSpell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData().FirstOrDefault(x => x.Effects.Any(e => e.EntityTypeId == EntityTypes.Unit && e.EntityId == unitType.IdKey));

            double quantity = 1;
            if (summonSpell != null)
            {
                quantity = _roleService.GetSpellScalingLevel(party, member, summonSpell);
            }

            quantity *= _gameData.Get<CrawlerCombatSettings>(_gs.ch).SummonQuantityScale;

            // 1.5 here for rounding and not random scaling value combat to combat

            if (_rand.NextDouble() < (quantity - (int)quantity))
            {
                quantity = Math.Ceiling(quantity);
            }

            long luckBonus = _crawlerStatService.GetStatBonus(party, member, StatTypes.Luck);

            long luckySummonCount = 0;
            for (int q = 0; q < quantity; q++)
            {
                if (_rand.NextDouble() * 100 < luckBonus)
                {
                    luckySummonCount++;
                }
            }
            quantity += luckySummonCount;

            return (int)Math.Max(1, Math.Sqrt(quantity));
        }

        public void PickRandomTarget(PartyData party, UnitAction newUnitAction)
        {
            if (newUnitAction.FinalTargets.Count < 1)
            {
                if (newUnitAction.FinalTargetGroups.Count > 0)
                {
                    foreach (CombatGroup cgroup in newUnitAction.FinalTargetGroups)
                    {
                        newUnitAction.FinalTargets.AddRange(cgroup.Units);
                    }
                }
                else if (newUnitAction.PossibleTargetUnits.Count > 0)
                {
                    newUnitAction.FinalTargets = newUnitAction.PossibleTargetUnits.ToList();
                }
                else
                {
                    if (newUnitAction.PossibleTargetGroups.Count > 0)
                    {
                        CombatGroup finalGroup = newUnitAction.PossibleTargetGroups[_rand.Next(newUnitAction.PossibleTargetGroups.Count)];
                        newUnitAction.FinalTargets = finalGroup.Units.ToList();
                    }
                }
            }
        }
    }
}

using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;
using Assets.Scripts.Crawler.ClientEvents.CombatEvents;
using Assets.Scripts.Crawler.ClientEvents.WorldPanelEvents;
using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.Items.Services;
using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Items.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Constants;
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
        Task CastSpell(PartyData party, UnitAction action, CancellationToken token);
        Task CastSpellOnNextTarget(PartyData party, UnitAction action, CancellationToken token);
        Task CastAllPartyBuffs(PartyData party, CancellationToken token);
        ISpecialMagicHelper GetSpecialEffectHelper(long effectEntityId);
        void RemoveSpellPowerCost(PartyData party, CrawlerUnit member, CrawlerSpell spell);
        void SetupCombatData(PartyData party, PartyMember member);
        long GetPowerCost(PartyData party, CrawlerUnit unit, CrawlerSpell spell);
        bool IsEnemyTarget(long targetTypeId);
        bool IsNonCombatTarget(long targetTypeId);
        long GetSummonQuantity(PartyData party, PartyMember member, UnitType unitType);
        void PickRandomTarget(PartyData party, UnitAction unitAction);
        float GetBuffPartyBuffPowerFromTier(double tier);
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
        private IRoleService _roleService = null;
        private IDispatcher _dispatcher = null;
        private IClientAppService _appService = null;
        private ICrawlerService _crawlerService = null;
        private IPartyService _partyService = null;
        private ICrawlerItemService _itemService = null;

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
        public FullSpell GetFullSpell(PartyData party, CrawlerUnit caster, CrawlerSpell spell, Item castingItem = null, long overrideLevel = 0)
        {

            FullSpell fullSpell = new FullSpell() { Spell = spell, CastingItem = castingItem };

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(null);

            RoleScalingType scalingType = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).Get(spell.RoleScalingTypeId);

            if (castingItem != null)
            {
                scalingType = null;
                double maxScalingBonus = 0;
                IReadOnlyList<RoleScalingType> scalingTypes = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).GetData();

                foreach (RoleScalingType rtype in scalingTypes)
                {
                    double maxStatBonus = _crawlerStatService.GetStatBonus(party, caster, rtype.ScalingStatTypeId);
                    if (maxStatBonus > maxScalingBonus)
                    {
                        maxScalingBonus = maxStatBonus;
                        scalingType = rtype;
                    }
                }
            }

            TargetType targetType = _gameData.Get<TargetTypeSettings>(_gs.ch).Get(spell.TargetTypeId);

            double critChance = 0;

            double attackQuantity = 0;

            if (castingItem == null && caster is PartyMember member)
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
                    elemType = elemSettings.Get(ElementTypes.Melee);
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

            double luckBonus = (long)(_crawlerStatService.GetStatBonus(party, caster, StatTypes.Luck) *
                combatSettings.LuckBonusHitChanceScale);

            if (caster.IsPlayer() && spell.CombatActionId == CombatActions.Attack)
            {
                luckBonus += party.Buffs.Get(PartyBuffs.LuckyBlade) * _gameData.Get<PartyBuffSettings>(_gs.ch).GetEffectScale(PartyBuffs.LuckyBlade);
            }

            long luckyAttackCount = 0;
            for (int a = 0; a < attackQuantity; a++)
            {
                if (_rand.NextDouble() * 100 < luckBonus)
                {
                    luckyAttackCount++;
                }
            }
            attackQuantity += luckyAttackCount;

            if (castingItem != null)
            {

            }

            if (overrideLevel > 0)
            {
                attackQuantity = overrideLevel;
                luckyAttackCount = 0;
            }

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



        public async Task CastSpell(PartyData party, UnitAction action, CancellationToken token)
        {
            try
            {

                long overrideLevel = 0;

                if (action.CastingItem != null)
                {
                    ItemEffect effect = action.CastingItem.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.CrawlerSpell &&
                    x.EntityId == action.Spell.IdKey);

                    if (effect != null)
                    {
                        overrideLevel = effect.Quantity;
                    }
                }

                action.IsComplete = true;
                if (action.Spell == null)
                {
                    return;
                }

                if (_combatService.IsDisabled(action.Caster))
                {
                    if (!action.Caster.StatusEffects.HasBit(StatusEffects.Dead))
                    {
                        ShowCombatLogText($"{action.Caster.Name} is disabled!");
                    }
                    return;
                }

                if (action.CastingItem == null && _combatService.IsActionBlocked(party, action.Caster, action.Spell.CombatActionId))
                {
                    ShowCombatLogText($"{action.Caster.Name} was blocked from performing that action!");
                    return;
                }

                if (action.CastingItem != null && _combatService.IsActionBlocked(party, action.Caster, CombatActions.UseItem))
                {
                    ShowCombatLogText($"{action.Caster.Name} was blocked from using an item.");
                    return;
                }

                if (action.CastingItem != null)
                {
                    _logService.Info("Used casting item: " + action.CastingItem.Name + " " + overrideLevel);
                }

                if (action.CastingItem == null && _combatService.ProccedStatusEffect(action.Caster, StatusEffects.Berserk))
                {
                    if (action.Caster.IsPlayer())
                    {
                        ShowCombatLogText($"{action.Caster.Name} is BERSERK!");
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
                                ShowCombatLogText($"{action.Caster.Name} could not find a target for {newSpell.Name}!");
                                return;
                            }
                            action = newUnitAction;
                        }
                    }
                }

                if (action.CastingItem == null && _combatService.ProccedStatusEffect(action.Caster, StatusEffects.Clumsy))
                {
                    ShowCombatLogText($"{action.Caster.Name} is Clumsy and fails to do anything!");
                    return;
                }

                if (action.CastingItem == null && _combatService.ProccedStatusEffect(action.Caster, StatusEffects.Confused))
                {
                    ShowCombatLogText($"{action.Caster.Name} is Confused and targets the wrong thing!");

                    UnitAction newUnitAction = _combatService.GetActionFromSpell(party, action.Caster, action.Spell);

                    PickRandomTarget(party, newUnitAction);

                    return;
                }

                action.SpellBeingCast = GetFullSpell(party, action.Caster, action.Spell, action.CastingItem, overrideLevel);

                bool foundOkTarget = false;
                if (!IsNonCombatTarget(action.SpellBeingCast.Spell.TargetTypeId))
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

                if (!action.SpellBeingCast.Spell.HasFlag(CrawlerSpellFlags.SuppressCastText) && action.SpellBeingCast.LuckyHitQuantity < 1)
                {
                    ShowCombatLogText($"{action.Caster.Name} casts {action.SpellBeingCast.Spell.Name}");
                    if (action.SpellBeingCast.LuckyHitQuantity == 1)
                    {
                        ShowCombatLogText(_textService.HighlightText("1 Lucky Hit!", TextColors.ColorGold));
                    }
                    else if (action.SpellBeingCast.LuckyHitQuantity > 1)
                    {
                        ShowCombatLogText(_textService.HighlightText($"{action.SpellBeingCast.LuckyHitQuantity} Lucky Hits!", TextColors.ColorGold));
                    }
                }

                if (action.CastingItem == null && action.Caster is PartyMember pmember)
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
                                action.FinalTargets.AddRange(group.Units.ToList());
                            }
                        }
                    }
                    action.FinalTargets = new List<CrawlerUnit>(action.FinalTargets.OrderBy(x => Guid.NewGuid().ToString()));
                }

                if (action.CastingItem != null)
                {
                    party.ItemsUsed.Add(action.CastingItem.Id);
                }

                await CastSpellOnNextTarget(party, action, token);
            }
            catch (Exception e)
            {
                _logService.Exception(e, "CastSpell");
            }
        }

        public async Task CastSpellOnNextTarget(PartyData party, UnitAction action, CancellationToken token)
        {

            if (_combatService.IsDisabled(action.Caster))
            {
                return;
            }
            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);
            while (action.FinalTargets.Count > 0 && action.SpellBeingCast != null && action.SpellBeingCast.HitsLeft > 0)
            {

                CrawlerUnit currTarget = null;

                if (action.Caster.FactionTypeId != FactionTypes.Player &&
                    _rand.NextDouble() < combatSettings.HitPartyRandomMemberChance)
                {
                    List<PartyMember> targets = party.GetActiveParty().Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead)).ToList();

                    if (targets.Count > 0)
                    {
                        currTarget = targets[_rand.Next(targets.Count)];
                        ShowCombatLogText(action.Caster.Name + " Targets " + currTarget.Name);
                    }
                }

                if (currTarget != null)
                {
                    action.FinalTargets.Remove(currTarget);
                }
                else
                {
                    currTarget = action.FinalTargets.Last();
                }

                action.FinalTargets.Remove(currTarget);
                if (party.Combat != null && currTarget.StatusEffects.HasBit(StatusEffects.Dead))
                {
                    continue;
                }

                long originalHitsLeft = action.SpellBeingCast.HitsLeft;
                string combatGroupId = currTarget.CombatGroupId;
                if (action.SpellBeingCast.Spell.TargetTypeId == TargetTypes.EnemyInEachGroup &&
                    currTarget.CombatGroupId != combatGroupId)
                {
                    action.SpellBeingCast.HitsLeft = originalHitsLeft;
                    combatGroupId = currTarget.CombatGroupId;
                }

                await CastSpellOnUnit(party, action.Caster, action.SpellBeingCast, currTarget, CrawlerCombatConstants.GetScrollingFrames(party.ScrollFramesIndex), token);

                if (party.Combat != null && party.Combat.AttackSequence != null &&
                    action.SpellBeingCast.HitsLeft > 0 && action.FinalTargets.Count > 0)
                {
                    action.Caster.CombatPriority *= (1 - _rand.NextDouble() * combatSettings.SubsequentAttackPriorityLossPercent);

                    bool didInsert = false;
                    for (int i = party.Combat.AttackSequence.Count - 1; i >= 0; i--)
                    {
                        if (party.Combat.AttackSequence[i].CombatPriority < action.Caster.CombatPriority)
                        {
                            didInsert = true;
                            party.Combat.AttackSequence.Insert(i + 1, action.Caster);
                            break;
                        }
                    }

                    if (!didInsert)
                    {
                        party.Combat.AttackSequence.Insert(0, action.Caster);
                    }
                }
                break;
            }
        }

        private void ShowCombatLogText(string text)
        {
            _dispatcher.Dispatch(new AddActionPanelText(text));
        }

        private void AddToActionDict(Dictionary<string, ActionListItem> dict, CrawlerUnit caster, CrawlerUnit target, string actionName, long quantity, long extraMessageBits, bool regularHit, ECombatTextTypes textType, long elementTypeId)
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
            dict[actionName].Target = target;
            dict[actionName].Caster = caster;

        }

        internal class ActionListItem
        {
            public long ElementTypeId { get; set; }
            public long TotalQuantity { get; set; }
            public long TotalHits { get; set; }
            public long ExtraMessageBits { get; set; }
            public bool IsRegularHit { get; set; }
            public ECombatTextTypes TextType { get; set; }
            public CrawlerUnit Caster { get; set; }
            public CrawlerUnit Target { get; set; }
        }

        public async Awaitable CastSpellOnUnit(PartyData party, CrawlerUnit caster, FullSpell spell, CrawlerUnit target, int delayFrames, CancellationToken token)
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

            float delayTime = (delayFrames * 1.0f) / _appService.TargetFrameRate;
            float afterInitialTextTime = Mathf.Max(0, delayTime - CrawlerClientCombatConstants.CombatDooberFlyTime);

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


            PartyBuffSettings partyBuffSettings = _gameData.Get<PartyBuffSettings>(_gs.ch);


            double autoHealValue = party.Buffs.Get(PartyBuffs.Autoheal);
            double barrierValue = party.Buffs.Get(PartyBuffs.Barrier);
            double thornsValue = party.Buffs.Get(PartyBuffs.Thorns);
            double defenseScale = _roleService.GetRoleScalingLevel(party, target, RoleScalingTypes.Defense);
            double sharpshooterValue = party.Buffs.Get(PartyBuffs.Sharpshooter);

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

                        ElementType etype = _gameData.Get<ElementTypeSettings>(_gs.ch).Get(effect.ElementTypeId);
                        double finalCritChance = hit.CritChance;

                        bool casterIgnoreResists = caster.FactionTypeId == FactionTypes.Player &&
                            _partyService.HasPartyBuff(party, EntityTypes.Element, effect.ElementTypeId);


                        bool targetHasResist = !casterIgnoreResists &&
                            (FlagUtils.IsSet(target.ResistBits, elementBits) ||
                            (target.FactionTypeId == FactionTypes.Player &&
                            _partyService.HasPartyBuff(party, EntityTypes.Resist, effect.ElementTypeId)));


                        if (targetHasResist)
                        {
                            if (!FlagUtils.IsSet(target.VulnBits, elementBits))
                            {
                                damageScale *= etype.ResistDamagePercent / 100.0;
                                finalCritChance += etype.ResistCritPercentMod;
                                extraMessageBits |= ExtraMessageBits.Resists;
                            }
                        }
                        else if (FlagUtils.IsSet(target.VulnBits, elementBits))
                        {
                            damageScale *= etype.VulnDamagePercent / 100.0;
                            extraMessageBits |= ExtraMessageBits.Vulnerable;
                            finalCritChance += etype.VulnCritPercentMod;
                        }

                        // Don't allow full crit chance per hit, too strong.
                        finalCritChance *= critChanceScaling;

                        if (!target.IsPlayer() && target.DefendRank == 0 && finalCritChance > 0 &&
                            _rand.NextDouble() * 100 < finalCritChance && weakReductionPercent == 0)
                        {
                            newQuantity = target.Stats.Curr(StatTypes.Health);
                            AddToActionDict(actionList, caster, target, "CRITS!", newQuantity, extraMessageBits, false, ECombatTextTypes.Damage, spell.Effects[0].ElementType.IdKey);
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
                                AddToActionDict(actionList, caster, target, "Misses", 0, ExtraMessageBits.Misses, false, ECombatTextTypes.None, 0);
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

                            if (target.FactionTypeId == FactionTypes.Player && _rand.NextDouble() * 100 < barrierValue * partyBuffSettings.GetProcChanceScale(PartyBuffs.Barrier))
                            {
                                long removedQuantity = Math.Min(newQuantity, (long)(newQuantity * defenseScale * partyBuffSettings.GetEffectScale(PartyBuffs.Barrier) / 100.0));
                                newQuantity -= removedQuantity;

                                if (removedQuantity > 0)
                                {
                                    AddToActionDict(actionList, target, target, "Absorbed", removedQuantity, 0, false, ECombatTextTypes.Defense, ElementTypes.Earth);
                                }
                            }

                            if (newQuantity > 0)
                            {
                                string actionWord = (effect.EntityTypeId == EntityTypes.Attack ? "Attacks" :
                                    effect.EntityTypeId == EntityTypes.Shoot ? "Shoots" :
                                        fullEffect.ElementType.ObserverActionName);
                                AddToActionDict(actionList, caster, target, actionWord, newQuantity, extraMessageBits, true, ECombatTextTypes.Damage, spell.Effects[0].ElementType.IdKey);
                            }
                        }

                        totalDamage += newQuantity;
                        _crawlerStatService.Add(party, target, StatTypes.Health, StatCategories.Curr, -newQuantity, effect.ElementTypeId);

                        // Sharpshooter do some extra damage.
                        if (currHitTimes == 0 && newQuantity > 0 && effect.EntityTypeId == EntityTypes.Shoot && caster.IsPlayer() &&
                            _rand.NextDouble() < sharpshooterValue * partyBuffSettings.GetProcChanceScale(PartyBuffs.Sharpshooter))
                        {

                            long effectTier = (long)(1 + _rand.NextDouble() * (sharpshooterValue * sharpshooterValue * partyBuffSettings.GetEffectScale(PartyBuffs.Sharpshooter)));

                            StatusEffect statusEffect = _gameData.Get<StatusEffectSettings>(_gs.ch).Get(effectTier);

                            if (statusEffect != null && statusEffect.IdKey < StatusEffects.Dead)
                            {

                                DisplayEffect displayEffect = new DisplayEffect()
                                {
                                    MaxDuration = effect.MaxQuantity,
                                    DurationLeft = effect.MaxQuantity, // MaxQuantity == 0 means infinite
                                    EntityTypeId = EntityTypes.StatusEffect,
                                    EntityId = effect.EntityId,
                                };
                                target.AddEffect(displayEffect);
                                fullAction = $"Accurate Hit! {target.Name} is affected by {statusEffect.Name}";
                            }

                        }
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
                        AddToActionDict(actionList, caster, target, "Heals", newQuantity, 0, false, ECombatTextTypes.Healing, 0);
                    }
                    else if (effect.EntityTypeId == EntityTypes.PartyBuff)
                    {
                        double tier = _roleService.GetRoleScalingLevel(party, caster, RoleScalingTypes.Utility);

                        party.Buffs.Set(effect.EntityId, GetBuffPartyBuffPowerFromTier(tier));
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

                                    if (target.IsPlayer() && _partyService.HasPartyBuff(party, EntityTypes.StatusEffect, effect.EntityId))
                                    {
                                        ShowCombatLogText($"The party is immune to {effect.Name}!");
                                    }
                                    else
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
                        ShowCombatLogText(fullAction);
                    }
                }
                currHitTimes++;
                spell.HitsLeft--;

                critChanceScaling *= combatSettings.CritScaledownPerHit;

                if (target.FactionTypeId == FactionTypes.Player)
                {
                    if (_rand.NextDouble() * 100 < partyBuffSettings.GetProcChanceScale(PartyBuffs.Autoheal) * autoHealValue)
                    {
                        double maxVal = autoHealValue * partyBuffSettings.GetEffectScale(PartyBuffs.Autoheal);

                        double healing = MathUtils.FloatRange(1, maxVal * maxVal, _rand);

                        currHealth = target.Stats.Curr(StatTypes.Health);
                        maxHealth = target.Stats.Max(StatTypes.Health);

                        healing = Math.Min(healing, maxHealth - currHealth);

                        int intHealing = (int)healing;

                        if (intHealing > 0)
                        {
                            AddToActionDict(actionList, target, target, "AutoHeals", intHealing, 0, false, ECombatTextTypes.Healing, ElementTypes.Earth);

                            _crawlerStatService.Add(party, target, StatTypes.Health, StatCategories.Curr, intHealing);
                        }
                    }
                }

                bool isDead = target.Stats.Curr(StatTypes.Health) <= 0;


                bool casterIsDead = false;
                if (spell.HitsLeft < 1 || isDead)
                {
                    if (target.FactionTypeId == FactionTypes.Player && totalDamage > 0 && thornsValue > 0)
                    {
                        long thornsDamage = (long)(totalDamage * thornsValue * partyBuffSettings.GetEffectScale(PartyBuffs.Thorns));

                        thornsDamage = Math.Min(thornsDamage, caster.Stats.Curr(StatTypes.Health));

                        if (thornsDamage > 0)
                        {
                            _crawlerStatService.Add(party, caster, StatTypes.Health, StatCategories.Curr, -thornsDamage);

                            casterIsDead = caster.Stats.Curr(StatTypes.Health) <= 0;

                            AddToActionDict(actionList, target, caster, "Retaliates Against", thornsDamage, 0, false, ECombatTextTypes.Thorns, ElementTypes.Earth);

                        }
                    }

                    bool didShowMisses = false;
                    List<string> actionListKeys = actionList.Keys.ToList();
                    for (int k = 0; k < actionListKeys.Count; k++)
                    {
                        ActionListItem actionListItem = actionList[actionListKeys[k]];

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

                        if (actionListItem.TextType != ECombatTextTypes.None && actionListItem.TotalQuantity != 0)
                        {
                            if (actionListItem.Caster == caster && actionListItem.Target == target)
                            {
                                ShowCombatDoober(actionListItem.Caster, actionListItem.Target, actionListItem.TotalQuantity, actionListItem.ElementTypeId, delayTime);
                            }
                            await Awaitable.WaitForSecondsAsync(delayTime, token);
                            ShowFloatingCombatText(actionListItem.Caster, actionListItem.Target,
                                ((actionListItem.TextType == ECombatTextTypes.Damage ||
                                actionListItem.TextType == ECombatTextTypes.Thorns)
                                ? "-" : "") + actionListItem.TotalQuantity,
                                actionListItem.TextType, actionListItem.ElementTypeId);
                        }
                        ShowCombatLogText($"{actionListItem.Caster.Name} {actionListKeys[k]} {actionListItem.Target.Name} {hitText}"
                            + (actionListItem.TotalQuantity > 0 ? $" for {actionListItem.TotalQuantity} " : "")
                            + " " + $"{extraWords}");
                        if (afterInitialTextTime > 0 && (k < actionListKeys.Count - 1 || !isDead))
                        {
                            await Awaitable.WaitForSecondsAsync(afterInitialTextTime, token);
                        }
                    }

                    await CheckHandleUnitDeath(party, caster, target, afterInitialTextTime, token);
                    await CheckHandleUnitDeath(party, target, caster, afterInitialTextTime, token);
                    _dispatcher.Dispatch(new UpdateCombatGroups());
                    break;
                }
            }

            await Task.CompletedTask;
        }

        private async Awaitable CheckHandleUnitDeath(PartyData party, CrawlerUnit caster, CrawlerUnit target, float afterInitialTextTime, CancellationToken token)
        {
            if (target.Stats.Curr(StatTypes.Health) > 0)
            {
                return;
            }
            ShowCombatLogText($"{target.Name} is DEAD!\n");
            ShowFloatingCombatText(caster, target, "DEAD!", ECombatTextTypes.Info, 0);
            if (afterInitialTextTime > 0)
            {
                await Awaitable.WaitForSecondsAsync(afterInitialTextTime, token);
            }
            target.StatusEffects.SetBit(StatusEffects.Dead);

            CombatGroup cg = party.Combat.Enemies.FirstOrDefault(x => x.Id == target.CombatGroupId);

            if (cg == null)
            {
                cg = party.Combat.Allies.FirstOrDefault(x => x.Id == target.CombatGroupId);
            }

            cg.Units.Remove(target);

            if (cg.FactionTypeId != FactionTypes.Player)
            {
                party.Combat.EnemiesKilled.Add(target);
            }
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
                ElementTypeId = elementTypeId,
            });
        }

        private void ShowCombatDoober(CrawlerUnit caster, CrawlerUnit target, long damage, long elementTypeId, float infoDelayTime)
        {
            if (caster != null && target != null && caster != target)
            {
                _dispatcher.Dispatch(new ShowCombatBolt()
                {
                    CasterId = caster.IsPlayer() ? caster.Id : caster.CombatGroupId,
                    TargetId = target.IsPlayer() ? target.Id : target.CombatGroupId,
                    ElementTypeId = elementTypeId,
                    Seconds = Math.Min(infoDelayTime, CrawlerClientCombatConstants.CombatDooberFlyTime),
                    SizeScale = Math.Max(1, 1 + Math.Log10(damage) / 3),
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
                        newUnitAction.FinalTargets.AddRange(cgroup.Units.ToList());
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

        public async Task CastAllPartyBuffs(PartyData party, CancellationToken token)
        {
            if (_crawlerService.GetState() != ECrawlerStates.ExploreWorld)
            {
                return;
            }

            IReadOnlyList<PartyBuff> allBuffs = _gameData.Get<PartyBuffSettings>(_gs.ch).GetData();

            Dictionary<PartyMember, List<CrawlerSpell>> spellDict = new Dictionary<PartyMember, List<CrawlerSpell>>();

            foreach (PartyMember member in party.GetActiveParty())
            {
                if (_combatService.IsDisabled(member))
                {
                    continue;
                }

                spellDict[member] = GetAbilitiesForMember(party, member, true);

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

                PartyMember bestCaster = null;
                MemberItemSpell memberItemSpell = null;
                double bestPower = 0;

                foreach (PartyMember member in spellDict.Keys)
                {
                    long mana = member.Stats.Curr(StatTypes.Mana);

                    if (spellDict[member].Any(x => x.IdKey == spell.IdKey))
                    {
                        long cost = GetPowerCost(party, member, spell);

                        if (cost > mana)
                        {
                            continue;
                        }

                        double power = _roleService.GetSpellScalingLevel(party, member, spell);

                        if (bestCaster == null || power > bestPower)
                        {
                            bestCaster = member;
                            bestPower = power;
                            memberItemSpell = null;
                        }
                    }

                    List<MemberItemSpell> itemSpellStart = _itemService.GetUsableItemsForMember(party, member);


                    foreach (MemberItemSpell itemSpell in itemSpellStart)
                    {
                        if (itemSpell.ChargesLeft < 1)
                        {
                            continue;
                        }

                        ItemEffect effect = itemSpell.UsableItem.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.CrawlerSpell && x.EntityId == spell.IdKey);

                        if (effect != null && effect.Quantity > bestPower)
                        {
                            bestCaster = member;
                            bestPower = effect.Quantity;
                            memberItemSpell = itemSpell;
                        }
                    }
                }

                if (bestCaster != null)
                {

                    float newTier = GetBuffPartyBuffPowerFromTier(bestPower);

                    if (party.Buffs.Get(pbuff.IdKey) > newTier - 0.001f)
                    {
                        continue;
                    }

                    UnitAction action = _combatService.GetActionFromSpell(party, bestCaster, spell, null, memberItemSpell?.UsableItem ?? null);

                    await CastSpell(party, action, token);
                    await Awaitable.NextFrameAsync(token);
                }
            }

            await Task.CompletedTask;
        }

        public float GetBuffPartyBuffPowerFromTier(double tier)
        {
            return (float)Math.Sqrt(tier);
        }
    }
}

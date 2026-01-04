using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;
using Assets.Scripts.Crawler.ClientEvents.CombatEvents;
using Assets.Scripts.Crawler.ClientEvents.WorldPanelEvents;
using Assets.Scripts.Crawler.Constants;
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
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Options.Constants;
using Genrpg.Shared.Crawler.Options.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.Training.Settings;
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
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spells.Helpers.SpellEffectHelpers;
using Genrpg.Shared.Spells.Procs.Entities;
using Genrpg.Shared.Spells.Procs.Interfaces;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Spells.Settings.Targets;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Genrpg.Shared.Crawler.Spells.Services
{
    public class ActionListItem
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

    public interface ICrawlerSpellService : IInjectable
    {
        List<CrawlerSpell> GetAbilitiesForMember(PartyData party, PartyMember member, bool chooseSpells);
        List<CrawlerSpell> GetSpellsForMember(PartyData party, PartyMember member);
        List<CrawlerSpell> GetNonSpellCombatActionsForMember(PartyData party, PartyMember member);
        Task CastSpell(PartyData party, UnitAction action, CancellationToken token);
        Task CastSpellOnNextTarget(PartyData party, UnitAction action, CancellationToken token);
        ISpecialMagicHelper GetSpecialEffectHelper(long effectEntityId);
        void RemoveSpellPowerCost(PartyData party, CrawlerUnit member, CrawlerSpell spell);
        void SetupCombatData(PartyData party, PartyMember member);
        long GetPowerCost(PartyData party, CrawlerUnit unit, CrawlerSpell spell);
        bool IsEnemyTarget(long targetTypeId);
        bool IsNonCombatTarget(long targetTypeId);
        bool IsGroupTarget(long targetTypeId);
        long GetSummonQuantity(PartyData party, PartyMember member, UnitType unitType);
        void PickRandomTarget(PartyData party, UnitAction unitAction);
        List<Role> RolesThatCanCast(long crawlerSpellId);
        string RolesThatCanCastString(long crawlerSpellId);
        void ShowCombatLogText(string text);
        void AddToActionDict(Dictionary<string, ActionListItem> dict, CrawlerUnit caster, CrawlerUnit target, string actionName, long quantity, long extraMessageBits, bool regularHit, ECombatTextTypes textType, long elementTypeId);
        Awaitable UpdateAtEndOfCombatRound(PartyData party, CancellationToken token);
    }


    public class ExtraMessageBits
    {
        public const long Resists = (1 << 0);
        public const long Vulnerable = (1 << 1);
        public const long Misses = (1 << 2);
    }

    public class CrawlerSpellService : ICrawlerSpellService
    {



        private ILogService _logService = null;
        private ICrawlerCombatService _combatService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IClientRandom _rand = null;
        private IDispatcher _dispatcher = null;
        protected ICrawlerStatService _crawlerStatService = null;
        private ITextService _textService = null;
        private IRoleService _roleService = null;
        private IClientAppService _appService = null;
        private IInfoService _infoService = null;
        private ICrawlerOptionsService _optionsService = null;

        private SetupDictionaryContainer<long, ISpecialMagicHelper> _specialMagicEffectHelpers = new SetupDictionaryContainer<long, ISpecialMagicHelper>();
        private SetupDictionaryContainer<long, ICrawlerSpellEffectHelper> _effectHelpers = new SetupDictionaryContainer<long, ICrawlerSpellEffectHelper>();

        public ISpecialMagicHelper GetSpecialEffectHelper(long specialEffectId)
        {
            if (_specialMagicEffectHelpers.TryGetValue(specialEffectId, out ISpecialMagicHelper specialEffectHelper))
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

        public List<CrawlerSpell> GetAbilitiesForMember(PartyData party,
            PartyMember member, bool chooseSpells)
        {
            EActionCategories actionCategory = party.GetActionCategory();

            CrawlerSpellSettings spellSettings = _gameData.Get<CrawlerSpellSettings>(null);

            IReadOnlyList<CrawlerSpell> allSpells = spellSettings.GetData();

            List<CrawlerSpell> castSpells = allSpells.Where(x =>
            (x.CombatActionId == CombatActions.Cast) == chooseSpells).ToList();

            List<CrawlerSpell> okSpells = new List<CrawlerSpell>();

            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(_gs.ch);

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

            List<Role> myRoles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(member.Roles);

            Dictionary<long, long> roleLevels = new Dictionary<long, long>();

            foreach (UnitRole urole in member.Roles)
            {
                roleLevels[urole.RoleId] = urole.Level;
            }

            foreach (CrawlerSpell spell in castSpells)
            {
                if (spell.IdKey < 1)
                {
                    continue;
                }

                if (!trainingSettings.AdvanceOneClassPerLevel)
                {

                    if (!roleScalingTiers.ContainsKey(spell.RoleScalingTypeId))
                    {
                        _logService.Info("Bad RoleScalingType on " + spell.Name + ": " + spell.RoleScalingTypeId);
                        continue;
                    }

                    if (spell.RoleScalingTier > roleScalingTiers[spell.RoleScalingTypeId])
                    {
                        continue;
                    }
                }
                else
                {

                    bool roleKnowsThis = false;

                    foreach (UnitRole role in member.Roles)
                    {
                        // if a player role knows this and is high enough level, add it to the list.
                        if (spell.RolesKnowingThis.Any(x => x.RoleId == role.RoleId))
                        {
                            roleKnowsThis = true;
                            break;
                        }
                    }

                    if (!roleKnowsThis)
                    {
                        continue;
                    }
                }

                if (_combatService.IsActionBlocked(party, member, spell.CombatActionId))
                {
                    continue;
                }

                if (!roleSettings.HasBonus(member.Roles, EntityTypes.CrawlerSpell, spell.IdKey))
                {
                    if (_optionsService.HasOption(party, CrawlerOptions.WholeParty) ||
                        !spell.Effects.FastAny(x => x.EntityTypeId == EntityTypes.SpecialMagic && x.EntityId == SpecialMagics.TownPortal))
                    {
                        continue;
                    }
                }

                if (actionCategory == EActionCategories.NonCombat)
                {
                    if (IsEnemyTarget(spell.TargetTypeId))
                    {
                        continue;
                    }

                    if (spell.Effects.FastAny(x => x.EntityTypeId == EntityTypes.Stat || x.EntityTypeId == EntityTypes.Unit))
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
                startFullEffectList.Add(new FullEffect() { Effect = effect, Chance = effect.Chance, InitialEffect = true });
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

                        if (party.Combat != null && !caster.IsPlayer())
                        {
                            if (proc.EntityTypeId == EntityTypes.StatusEffect && proc.EntityId > party.Combat.MaxDebuffTier)
                            {
                                continue;
                            }
                        }

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

                action.DidCast = true;
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

                            if (newUnitAction != null)
                            {
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
                    //action.FinalTargets = new List<CrawlerUnit>(action.FinalTargets.OrderBy(x => Guid.NewGuid().ToString()));
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
                    List<PartyMember> targets = party.ActiveParty.Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead)).ToList();

                    if (targets.Count > 0)
                    {
                        currTarget = targets[_rand.Next(targets.Count)];
                        ShowCombatLogText(action.Caster.Name + " Targets " + currTarget.Name);
                    }
                }
                else
                {
                    action.FinalTargets = action.FinalTargets.Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead)).ToList();
                }
                if (action.FinalTargets.Count > 0)
                {
                    if (currTarget == null)
                    {
                        currTarget = action.FinalTargets.Last();
                    }
                    action.FinalTargets.Remove(currTarget);

                    long originalHitsLeft = action.SpellBeingCast.HitsLeft;
                    string combatGroupId = currTarget.CombatGroupId;
                    if (action.SpellBeingCast.Spell.TargetTypeId == TargetTypes.EnemyInEachGroup &&
                        currTarget.CombatGroupId != combatGroupId)
                    {
                        action.SpellBeingCast.HitsLeft = originalHitsLeft;
                        combatGroupId = currTarget.CombatGroupId;
                    }

                    if (IsGroupTarget(action.SpellBeingCast.Spell.TargetTypeId))
                    {
                        action.SpellBeingCast.HitsLeft = action.SpellBeingCast.HitQuantity;
                    }

                    await CastSpellOnUnit(party, action.Caster, action.SpellBeingCast, currTarget, token);
                }

                if (combatSettings.SubsequentAttackPriorityLossPercent > 0)
                {
                    if (party.Combat != null && party.Combat.AttackSequence != null)
                    {
                        if (action.FinalTargets.Count > 0)
                        {
                            if (IsGroupTarget(action.Spell.TargetTypeId))
                            {
                                action.SpellBeingCast.HitsLeft = action.SpellBeingCast.HitQuantity;
                            }

                            if (action.Spell.TargetTypeId == TargetTypes.EnemyInEachGroup)
                            {
                                string currentGroupId = currTarget.CombatGroupId;
                                if (action.SpellBeingCast.HitsLeft < 1)
                                {
                                    action.FinalTargets = action.FinalTargets.Where(x => x.CombatGroupId != currentGroupId).ToList();
                                    if (action.FinalTargets.Count > 0)
                                    {
                                        action.SpellBeingCast.HitsLeft = action.SpellBeingCast.HitQuantity;
                                    }
                                }
                            }
                        }

                        if (action.SpellBeingCast.HitsLeft < 1 || action.FinalTargets.Count < 1)
                        {
                            action.Caster.CombatActions.Remove(action);
                            action.Caster.ActionsThisRound--;
                        }

                        if (action.Caster.CombatActions.Count > 0)
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
                    }
                    break;
                }
            }
        }

        public void ShowCombatLogText(string text)
        {
            _dispatcher.Dispatch(new AddActionPanelText(text));
        }

        public void AddToActionDict(Dictionary<string, ActionListItem> dict, CrawlerUnit caster, CrawlerUnit target, string actionName, long quantity, long extraMessageBits, bool regularHit, ECombatTextTypes textType, long elementTypeId)
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

        public async Awaitable CastSpellOnUnit(PartyData party, CrawlerUnit caster, FullSpell spell, CrawlerUnit target, CancellationToken token)
        {
            if (IsGroupTarget(spell.Spell.TargetTypeId))
            {
                spell.HitsLeft = Math.Max(spell.HitQuantity, 1);
            }
            if (caster.StatusEffects.HasBit(StatusEffects.Cursed))
            {
                spell.HitsLeft = Math.Max(1, spell.HitsLeft / 2);
            }

            ApplyEffectArgs args = new ApplyEffectArgs();

            args.IsEnemyTarget = IsEnemyTarget(spell.Spell.TargetTypeId);

            if (args.IsEnemyTarget && target.StatusEffects.HasBit(StatusEffects.Dead))
            {
                return;
            }

            args.DelayTime = (CrawlerCombatConstants.GetScrollingFrames(party.ScrollFramesIndex) * 1.0f) / 30.0f;

            args.BuffSettings = _gameData.Get<PartyBuffSettings>(_gs.ch);
            args.AfterInitialTextTime = Mathf.Max(0.034f, args.DelayTime - CrawlerClientCombatConstants.CombatDooberFlyTime);
            args.CritChanceScaling = 1.0f;

            bool haveMultiHitEffect = spell.Effects.FastAny(x =>
            x.Effect.EntityTypeId == EntityTypes.Damage ||
            x.Effect.EntityTypeId == EntityTypes.Healing ||
            x.Effect.EntityTypeId == EntityTypes.Attack ||
            x.Effect.EntityTypeId == EntityTypes.Shoot);
            if (!haveMultiHitEffect)
            {
                spell.HitsLeft = 1;
            }


            while (spell.HitsLeft > 0)
            {
                if (args.IsEnemyTarget)
                {
                    double parryValue = party.Buffs[PartyBuffs.Parry];
                    if (args.IsEnemyTarget && target.IsPlayer() &&
                        _rand.NextDouble() < parryValue * args.BuffSettings.GetProcChanceScale(PartyBuffs.Parry))
                    {
                        AddToActionDict(args.ActionList, caster, target, "Parries", 1, 0, true, ECombatTextTypes.Info, ElementTypes.Earth);
                        args.DidParry = true;
                    }
                }

                foreach (FullEffect fullEffect in spell.Effects)
                {
                    if (!args.DidParry)
                    {
                        if (_rand.NextDouble() > fullEffect.Chance)
                        {
                            continue;
                        }

                        if (args.DidKill)
                        {
                            break;
                        }

                        args.NewQuantity = 0;
                        args.FullAction = null;
                        args.ExtraMessageBits = 0;

                        if (_effectHelpers.TryGetValue(fullEffect.Effect.EntityTypeId, out ICrawlerSpellEffectHelper helper))
                        {
                            await helper.ApplyEffectToUnit(party, args, spell, fullEffect, caster, target, token);
                        }
                    }
                    if (!string.IsNullOrEmpty(args.FullAction))
                    {
                        ShowCombatLogText(args.FullAction);
                    }
                }


                args.CritChanceScaling *= _gameData.Get<CrawlerCombatSettings>(_gs.ch).CritScaledownPerHit;

                if (target.FactionTypeId == FactionTypes.Player)
                {
                    double autoHealValue = party.Buffs[PartyBuffs.Autoheal];
                    if (_rand.NextDouble() * 100 < args.BuffSettings.GetProcChanceScale(PartyBuffs.Autoheal) * autoHealValue)
                    {
                        double maxVal = autoHealValue * args.BuffSettings.GetEffectScale(PartyBuffs.Autoheal);

                        double healing = MathUtils.FloatRange(1, maxVal * maxVal, _rand);

                        long currHealth = target.Stats.Curr(StatTypes.Health);
                        long maxHealth = target.Stats.Max(StatTypes.Health);

                        healing = Math.Min(healing, maxHealth - currHealth);

                        int intHealing = (int)healing;

                        if (intHealing > 0)
                        {
                            AddToActionDict(args.ActionList, target, target, "AutoHeals", intHealing, 0, false, ECombatTextTypes.Healing, ElementTypes.Earth);

                            _crawlerStatService.Add(party, target, StatTypes.Health, StatCategories.Curr, intHealing);
                        }
                    }
                }

                args.CurrHitTimes++;
                spell.HitsLeft--;

                bool isDead = target.Stats.Curr(StatTypes.Health) <= 0;

                bool casterIsDead = false;
                if (spell.HitsLeft < 1 || isDead)
                {
                    double retaliateValue = party.Buffs[PartyBuffs.Retaliate];
                    if (target.FactionTypeId == FactionTypes.Player && args.TotalDamage > 0 && retaliateValue > 0)
                    {
                        long thornsDamage = (long)(args.TotalDamage * retaliateValue * args.BuffSettings.GetEffectScale(PartyBuffs.Retaliate));

                        thornsDamage = Math.Min(thornsDamage, caster.Stats.Curr(StatTypes.Health));

                        if (thornsDamage > 0)
                        {
                            _crawlerStatService.Add(party, caster, StatTypes.Health, StatCategories.Curr, -thornsDamage);

                            casterIsDead = caster.Stats.Curr(StatTypes.Health) <= 0;

                            AddToActionDict(args.ActionList, target, caster, "Retaliates Against", thornsDamage, 0, false, ECombatTextTypes.Thorns, ElementTypes.Earth);

                        }
                    }


                    if (caster is PartyMember member)
                    {
                        if (args.TotalDamage > 0)
                        {
                            double lifeStealValue = party.Buffs[PartyBuffs.Lifesteal];
                            if (_rand.NextDouble() * 100 < args.BuffSettings.GetProcChanceScale(PartyBuffs.Lifesteal) * lifeStealValue)
                            {
                                long totalLifesteal = (long)(args.TotalDamage * args.BuffSettings.GetEffectScale(PartyBuffs.Lifesteal));

                                if (totalLifesteal > 0)
                                {
                                    _crawlerStatService.Add(party, caster, StatTypes.Health, StatCategories.Curr, totalLifesteal);
                                    AddToActionDict(args.ActionList, caster, target, "Steals Life From", totalLifesteal, 0, false, ECombatTextTypes.Healing, ElementTypes.Shadow);
                                }
                            }

                            double dotValue = party.Buffs[PartyBuffs.ApplyDoT];
                            if (_rand.NextDouble() * 100 < args.BuffSettings.GetProcChanceScale(PartyBuffs.ApplyDoT) * dotValue)
                            {
                                long totalDot = (long)(args.TotalDamage * args.BuffSettings.GetEffectScale(PartyBuffs.ApplyDoT));

                                if (totalDot > 0)
                                {
                                    target.DoTDamage += totalDot;
                                }
                            }
                        }
                    }

                    bool didShowMisses = false;

                    List<string> actionListKeys = args.ActionList.Keys.ToList();
                    for (int k = 0; k < actionListKeys.Count; k++)
                    {
                        bool showingMissNow = false;
                        ActionListItem actionListItem = args.ActionList[actionListKeys[k]];

                        string extraWords = "";

                        if (FlagUtils.IsSet(actionListItem.ExtraMessageBits, ExtraMessageBits.Misses))
                        {
                            if (actionListKeys.Count > 1)
                            {
                                continue;
                            }
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
                            foreach (ActionListItem item in args.ActionList.Values)
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
                            showingMissNow = true;
                            didShowMisses = true;
                        }

                        if (actionListItem.TextType != ECombatTextTypes.None && (actionListItem.TotalQuantity != 0 || showingMissNow))
                        {
                            if (actionListItem.Caster == caster && actionListItem.Target == target)
                            {
                                ShowCombatDoober(actionListItem.Caster, actionListItem.Target, actionListItem.TotalQuantity, actionListItem.ElementTypeId, args.DelayTime);
                            }
                            await Awaitable.WaitForSecondsAsync(args.DelayTime, token);
                            ShowFloatingCombatText(actionListItem.Caster, actionListItem.Target,
                                ((actionListItem.TextType == ECombatTextTypes.Damage ||
                                actionListItem.TextType == ECombatTextTypes.Thorns)
                                ? "-" : "") + actionListItem.TotalQuantity,
                                actionListItem.TextType, actionListItem.ElementTypeId);
                        }
                        ShowCombatLogText($"{actionListItem.Caster.Name} {actionListKeys[k]} {actionListItem.Target.Name} {hitText}"
                            + (actionListItem.TotalQuantity > 0 ? $" for {actionListItem.TotalQuantity} " : "")
                            + " " + $"{extraWords}");
                        if (args.AfterInitialTextTime > 0 && (k < actionListKeys.Count - 1 || !isDead))
                        {
                            await Awaitable.WaitForSecondsAsync(args.AfterInitialTextTime, token);
                        }
                    }

                    await CheckHandleUnitDeath(party, caster, target, args.AfterInitialTextTime, token);
                    await CheckHandleUnitDeath(party, target, caster, args.AfterInitialTextTime, token);
                    _dispatcher.Dispatch(new UpdateCombatGroups());
                    break;
                }
            }
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

            CombatGroup cg = party.Combat.GetGroup(target.CombatGroupId);

            if (cg != null)
            {
                cg.Units.Remove(target);

                if (cg.FactionTypeId != FactionTypes.Player)
                {
                    party.Combat.EnemiesKilled.Add(target);
                }
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
            CrawlerSpell summonSpell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData().FirstOrDefault(x => x.Effects.FastAny(e => e.EntityTypeId == EntityTypes.Unit && e.EntityId == unitType.IdKey));

            double quantity = 1;
            if (summonSpell != null)
            {
                quantity = _roleService.GetSpellScalingLevel(party, member, summonSpell);
            }

            quantity *= _gameData.Get<CrawlerCombatSettings>(_gs.ch).SummonQuantityScale;

            if (!_optionsService.HasOption(party, CrawlerOptions.WholeParty))
            {
                quantity *= 2;
            }

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
            if (newUnitAction == null || newUnitAction.FinalTargets == null)
            {
                return;
            }

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


        public List<Role> RolesThatCanCast(long crawlerSpellId)
        {
            return _gameData.Get<RoleSettings>(_gs.ch).GetData().Where(x => x.BinaryBonuses.FastAny(b => b.EntityTypeId == EntityTypes.CrawlerSpell && b.EntityId == crawlerSpellId)).ToList();
        }

        public string RolesThatCanCastString(long crawlerSpellId)
        {
            List<Role> castingRoles = RolesThatCanCast(crawlerSpellId);

            StringBuilder sb = new StringBuilder();

            sb.Append("Cast By: ");

            for (int r = 0; r < castingRoles.Count; r++)
            {
                sb.Append(_infoService.CreateInfoLink(castingRoles[r]) + (r < castingRoles.Count - 1 ? ", " : ""));
            }

            return sb.ToString();
        }

        public bool IsGroupTarget(long targetTypeId)
        {
            return targetTypeId == TargetTypes.AllAllies ||
                targetTypeId == TargetTypes.AllEnemies ||
                targetTypeId == TargetTypes.OneEnemyGroup;

        }

        public async Awaitable UpdateAtEndOfCombatRound(PartyData party, CancellationToken token)
        {
            if (party.Combat == null)
            {
                return;
            }

            List<CrawlerUnit> units = party.Combat.GetAllUnits();
            foreach (CrawlerUnit unit in units)
            {
                if (unit.DoTDamage > 0)
                {
                    _crawlerStatService.Add(party, unit, StatTypes.Health, StatCategories.Curr, -unit.DoTDamage);
                    ShowFloatingCombatText(unit, unit, (-unit.DoTDamage).ToString(), ECombatTextTypes.Damage, ElementTypes.Melee);

                    await CheckHandleUnitDeath(party, unit, unit, 0, token);
                    await Awaitable.NextFrameAsync(token);
                }
            }
        }
    }
}



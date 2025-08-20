using Assets.Scripts.Crawler.ClientEvents.CombatEvents;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Monsters.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Quests.Services;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.TimeOfDay.Constants;
using Genrpg.Shared.Crawler.TimeOfDay.Services;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Combat.Services
{
    public interface ICrawlerCombatService : IInitializable
    {
        Task<bool> StartCombat(PartyData party);
        Task EndCombatRound(PartyData party);
        bool SetMonsterActions(PartyData party);
        bool ReadyForCombat(PartyData party);
        bool IsDisabled(CrawlerUnit unit);
        bool IsActionBlocked(PartyData party, CrawlerUnit unit, long combatActionId);
        long GetWeakReductionPercent(CrawlerUnit unit, long combatActionId);
        List<UnitAction> GetActionsForPlayer(PartyData party, CrawlerUnit unit);
        UnitAction GetActionFromSpell(PartyData party, CrawlerUnit unit, CrawlerSpell spell,
            List<UnitAction> currentActions = null);
        void SetInitialActions(PartyData party);
        void AddCombatUnits(PartyData party, InitialCombatGroup initial);
        void EndCombat(PartyData party);
        string ShowGroupStatus(CombatGroup group);
        int GetMaxGroupSize(long level, double difficulty = 1.0f);
        FullMonsterStats GetFullMonsterStats(PartyData party, UnitType unitType, long factionTypeId, long combatLevel, bool isForCombat);
        bool ProccedStatusEffect(CrawlerUnit unit, long statusEffectId);
    }
    public class CrawlerCombatService : ICrawlerCombatService
    {
        private ICrawlerStatService _statService = null;
        private ICrawlerSpellService _crawlerSpellService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IClientRandom _rand = null;
        private ICrawlerMapService _crawlerMapService = null;
        private ICrawlerService _crawlerService = null;
        private ICrawlerWorldService _worldService = null;
        private ILogService _logService = null;
        private ITimeOfDayService _timeService = null;
        private IRoleService _roleService = null;
        private IDispatcher _dispatcher = null;
        private ICrawlerMoveService _moveService = null;
        private IInfoService _infoService = null;
        private ICrawlerQuestService _questService = null;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public void EndCombat(PartyData party)
        {
            party.Combat = null;

            IReadOnlyList<StatusEffect> statusEffects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

            foreach (PartyMember member in party.GetActiveParty())
            {
                foreach (StatusEffect effect in statusEffects)
                {
                    if (effect.RemoveAtEndOfCombat)
                    {
                        member.StatusEffects.RemoveBit(effect.IdKey);
                    }
                }
            }

            _dispatcher.Dispatch(new UpdateCombatGroups());
        }

        public int GetMaxGroupSize(long level, double difficulty = 1.0f)
        {
            StartCombatSettings startSettings = _gameData.Get<StartCombatSettings>(_gs.ch);

            return (int)Math.Min(startSettings.MaxGroupSize, (startSettings.BaseGroupSizeLevelCap + difficulty * startSettings.MaxGroupSizePerLevel));
        }

        public async Task<bool> StartCombat(PartyData party)
        {
            if (party.Combat != null)
            {
                return true;
            }

            InitialCombatState initialState = party.InitialCombat;

            if (initialState == null)
            {
                initialState = new InitialCombatState();
                party.InitialCombat = initialState;
            }
            party.NextId = 0;

            if (initialState.Level < 1)
            {
                initialState.Level = await _worldService.GetMapLevelAtParty(party);
            }

            CrawlerCombatState combatState = new CrawlerCombatState() { Level = initialState.Level };

            List<PartyMember> members = party.GetActiveParty();

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            StartCombatSettings startSettings = _gameData.Get<StartCombatSettings>(_gs.ch);

            List<InitialCombatGroup> partySummons = new List<InitialCombatGroup>();

            foreach (PartyMember member in members)
            {
                if (member.Summons.Count > 0 && !IsDisabled(member))
                {
                    foreach (PartySummon summon in member.Summons)
                    {
                        UnitType unitType = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(summon.UnitTypeId);

                        if (unitType != null)
                        {
                            long summonQuantity = _crawlerSpellService.GetSummonQuantity(party, member, unitType);

                            InitialCombatGroup icg = new InitialCombatGroup()
                            {
                                UnitTypeId = unitType.IdKey,
                                Quantity = summonQuantity,
                                Level = member.Level,
                                FactionTypeId = FactionTypes.Player,
                                Range = CrawlerCombatConstants.MinRange,
                            };

                            partySummons.Add(icg);
                        }
                    }
                }
            }

            CombatGroup partyGroup = new CombatGroup() { SingularName = "Player", PluralName = "Players", Id = party.GetNextId("PG") };
            combatState.Allies.Add(partyGroup);
            combatState.PartyGroup = partyGroup;

            IReadOnlyList<UnitType> allUnitTypes = _gameData.Get<UnitTypeSettings>(null).GetData();

            foreach (PartyMember member in members)
            {
                partyGroup.Units.Add(member);
                member.CombatGroupId = partyGroup.Id;
            }

            if (initialState.CombatGroups.Count < 1)
            {
                CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

                List<ZoneUnitSpawn> spawns = await _worldService.GetSpawnsAtPoint(party, party.CurrPos.MapId, party.CurrPos.X, party.CurrPos.Z);

                long level = party.InitialCombat.Level;

                double difficulty = (long)Math.Max(1, level * initialState.Difficulty);

                double groupSizeIncreaseChance = MathUtils.Clamp(startSettings.BaseGroupSizeIncreaseChance,
                    startSettings.BaseGroupSizeIncreaseChance + startSettings.GroupSizeIncreaseChancePerLevel * difficulty,
                    startSettings.MaxGroupSizeIncreaseChance);

                long maxGroupSize = GetMaxGroupSize(level, difficulty);

                int groupCount = CrawlerCombatConstants.StartGroupCount;

                double groupCountIncreaseChance = MathUtils.Clamp(startSettings.BaseGroupCountIncreaseChance,
                    startSettings.BaseGroupCountIncreaseChance + startSettings.GroupCountIncreaseChancePerLevel * difficulty,
                    startSettings.MaxGroupCountIncreaseChance);

                while (_rand.NextDouble() < groupCountIncreaseChance && groupCount < startSettings.MaxGroupCount)
                {
                    groupCount++;
                }

                int maxGroupCount = (int)Math.Min(startSettings.MaxGroupCount, CrawlerCombatConstants.StartGroupCount + (int)(startSettings.MaxGroupCountPerLevel * difficulty));

                groupCount = Math.Min(groupCount, maxGroupCount);

                List<UnitType> chosenUnitTypes = new List<UnitType>();

                while (chosenUnitTypes.Count < groupCount && spawns.Count > 0)
                {
                    ZoneUnitSpawn chosenSpawn = null;

                    if (_rand.NextDouble() > startSettings.SelectRandomUnitForCombatGroupChance)
                    {
                        double chanceSum = spawns.Sum(x => x.Weight);

                        double chanceChosen = _rand.NextDouble() * chanceSum;

                        foreach (ZoneUnitSpawn sp in spawns)
                        {
                            chanceChosen -= sp.Weight;
                            if (chanceChosen <= 0)
                            {
                                chosenSpawn = sp;
                                break;
                            }
                        }
                    }
                    else
                    {
                        chosenSpawn = spawns[_rand.Next() % spawns.Count];
                    }

                    UnitType newUnitType = allUnitTypes.FirstOrDefault(x => x.IdKey == chosenSpawn.UnitTypeId);
                    if (newUnitType != null && newUnitType.MinLevel <= level)
                    {
                        chosenUnitTypes.Add(newUnitType);
                    }
                    spawns.Remove(chosenSpawn);
                }

                List<UnitType> unitTypes = await _questService.GetKillQuestTargets(party);

                unitTypes = unitTypes.OrderBy(x => HashUtils.NewUUId()).ToList();

                if (unitTypes.Count > 0 && !chosenUnitTypes.Contains(unitTypes[0]))
                {
                    if (chosenUnitTypes.Count > 0)
                    {
                        chosenUnitTypes.RemoveAt(0);
                    }
                    chosenUnitTypes.Insert(0, unitTypes[0]);
                }

                int currRange = CrawlerCombatConstants.MinRange;

                foreach (UnitType unitType in chosenUnitTypes)
                {

                    long quantity = MathUtils.LongRange(CrawlerCombatConstants.StartGroupSize, startSettings.MaxStartGroupSize, _rand);

                    while (_rand.NextDouble() < groupSizeIncreaseChance && quantity < startSettings.MaxGroupSize)
                    {
                        quantity += MathUtils.LongRange(quantity / 2, quantity, _rand);
                        quantity += MathUtils.LongRange(CrawlerCombatConstants.StartGroupSize, startSettings.GroupSizeIncrement, _rand);
                    }

                    quantity = Math.Min(startSettings.MaxGroupSize, quantity);

                    if (quantity > maxGroupSize)
                    {
                        quantity = MathUtils.LongRange(maxGroupSize / 2, maxGroupSize, _rand);
                    }

                    InitialCombatGroup initialGroup = new InitialCombatGroup()
                    {
                        UnitTypeId = unitType.IdKey,
                        Range = currRange,
                        Quantity = quantity,
                        FactionTypeId = FactionTypes.Faction1,
                        Level = combatState.Level,
                    };

                    initialState.CombatGroups.Add(initialGroup);

                    if (_rand.NextDouble() < 0.6f)
                    {
                        currRange += CrawlerCombatConstants.RangeDelta;

                        if (_rand.NextDouble() < 0.2f)
                        {
                            currRange += CrawlerCombatConstants.RangeDelta;
                        }
                    }
                }
            }

            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            WorldQuestItem wqi = world.QuestItems.FirstOrDefault(x => x.IdKey == initialState.WorldQuestItemId);

            if (wqi != null && wqi.GuardUnitTypeId > 0 &&
                !string.IsNullOrEmpty(wqi.GuardName))
            {
                while (initialState.CombatGroups.Count > 3)
                {
                    initialState.CombatGroups.RemoveAt(0);
                }

                initialState.CombatGroups.Add(new InitialCombatGroup()
                {
                    UnitTypeId = wqi.GuardUnitTypeId,
                    Range = CrawlerCombatConstants.MaxRange,
                    Quantity = MathUtils.IntRange(5, 10, _rand),
                    Level = combatState.Level,
                    FactionTypeId = FactionTypes.Faction1,
                });

                initialState.CombatGroups.Add(new InitialCombatGroup()
                {
                    UnitTypeId = wqi.GuardUnitTypeId,
                    Range = CrawlerCombatConstants.MaxRange,
                    Quantity = 1,
                    Level = combatState.Level + 7 + combatState.Level / 10,
                    FactionTypeId = FactionTypes.Faction1,
                    BossName = wqi.GuardName
                });
            }

            // Now save party so players have to come back and fight the monsters even if they quit.
            await _crawlerService.SaveGame();
            party.Combat = combatState;
            party.InitialCombat = null;

            foreach (InitialCombatGroup allyGroup in partySummons)
            {
                AddCombatUnits(party, allyGroup);
            }

            foreach (InitialCombatGroup initialGroup in initialState.CombatGroups)
            {
                UnitType unitType = allUnitTypes.FirstOrDefault(x => x.IdKey == initialGroup.UnitTypeId);

                AddCombatUnits(party, initialGroup);
            }

            LastMoveStatus status = _moveService.GetLastMoveStatus();
            status.MovesSinceLastCombat = 0;
            _dispatcher.Dispatch(new UpdateCombatGroups());
            return true;
        }

        public FullMonsterStats GetFullMonsterStats(PartyData party, UnitType unitType, long factionTypeId, long combatLevel, bool isForCombat)
        {
            FullMonsterStats retval = new FullMonsterStats();


            IReadOnlyList<CrawlerSpell> crawlerSpells = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData();

            List<long> defendSpellIds = crawlerSpells.Where(x => x.CombatActionId == CombatActions.Defend).Select(x => x.IdKey).ToList();

            IReadOnlyList<StatusEffect> statusEffects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);

            CrawlerMonsterSettings monsterSettings = _gameData.Get<CrawlerMonsterSettings>(_gs.ch);

            ElementTypeSettings elementSettings = _gameData.Get<ElementTypeSettings>(_gs.ch);

            IReadOnlyList<UnitKeyword> allUnitKeywords = _gameData.Get<UnitKeywordSettings>(_gs.ch).GetData();

            List<UnitEffect> spells = new List<UnitEffect>();
            List<UnitEffect> applyEffects = new List<UnitEffect>();

            List<UnitEffect> resistEffects = new List<UnitEffect>();
            List<UnitEffect> vulnEffects = new List<UnitEffect>();

            long suffixKeywordId = 0;
            UnitKeyword suffixKeyword = null;

            if (_rand.NextDouble() < monsterSettings.UnitKeywordChance && unitType.Keywords.Count > 0)
            {
                List<long> possibleKeywordIds = unitType.Keywords.Select(x => x.UnitKeywordId).ToList();

                List<UnitKeyword> possibleKeywords = allUnitKeywords.Where(x => possibleKeywordIds.Contains(x.IdKey)).ToList();

                if (possibleKeywords.Count > 0)
                {
                    UnitKeyword chosenKeyword = RandomUtils.GetRandomElement(possibleKeywords, _rand);

                    suffixKeywordId = chosenKeyword.IdKey;
                    suffixKeyword = chosenKeyword;
                }

            }

            List<UnitKeyword> extraKeywords = new List<UnitKeyword>();
            List<CurrentUnitKeyword> extraCurrentKeyWords = new List<CurrentUnitKeyword>();
            if (isForCombat && factionTypeId != FactionTypes.Player)
            {
                CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

                List<CurrentUnitKeyword> okMapKeywords = map.UnitKeywords.Where(k => k.UnitKeywordId != suffixKeywordId).ToList();

                if (okMapKeywords.Count > 0 && _rand.NextDouble() < monsterSettings.MapUnitKeywordChance)
                {
                    CurrentUnitKeyword mapKeyword = okMapKeywords[_rand.Next(okMapKeywords.Count)];

                    UnitKeyword unitKeyword = allUnitKeywords.FirstOrDefault(x => x.IdKey == mapKeyword.UnitKeywordId);
                    if (unitKeyword != null)
                    {
                        extraCurrentKeyWords.Add(mapKeyword);
                        extraKeywords.Add(unitKeyword);
                    }
                }
            }

            if (suffixKeywordId > 0)
            {
                UnitKeyword keyword = allUnitKeywords.FirstOrDefault(x => x.IdKey == suffixKeywordId);
                if (keyword != null && !extraKeywords.Contains(keyword))
                {
                    retval.Range = Math.Max(keyword.MinRange, retval.Range);
                    spells.AddRange(keyword.Effects.Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell));
                    applyEffects.AddRange(keyword.Effects.Where(x => x.EntityTypeId == EntityTypes.StatusEffect));
                    resistEffects.AddRange(keyword.Effects.Where(x => x.EntityTypeId == EntityTypes.Resist));
                    vulnEffects.AddRange(keyword.Effects.Where(x => x.EntityTypeId == EntityTypes.Vulnerability));
                    extraKeywords.Add(keyword);
                }
            }

            spells.AddRange(unitType.Effects.Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell));
            applyEffects.AddRange(unitType.Effects.Where(x => x.EntityTypeId == EntityTypes.StatusEffect));
            resistEffects.AddRange(unitType.Effects.Where(x => x.EntityTypeId == EntityTypes.Resist));
            vulnEffects.AddRange(unitType.Effects.Where(x => x.EntityTypeId == EntityTypes.Vulnerability));

            // Remove duplicates
            spells = spells.GroupBy(x => x.EntityId).Select(g => g.First()).ToList();
            applyEffects = applyEffects.GroupBy(x => x.EntityId).Select(g => g.First()).ToList();

            List<FullEffect> finalApplyEffects = new List<FullEffect>();

            StatusEffectSettings statusSettings = _gameData.Get<StatusEffectSettings>(_gs.ch);

            // Don't introduce stronger debuffs until later when the player have a chance to cure them.
            double maxEffectTier = combatSettings.DebuffTiersPerUnitLevel * combatLevel;
            foreach (UnitEffect aeffect in applyEffects)
            {
                if (aeffect.EntityId > maxEffectTier)
                {
                    continue;
                }

                double levelDiff = maxEffectTier - aeffect.EntityId;

                // Chance is based on how much higher this level is than the debuff id/difficulty.
                double effectChance = Math.Min(combatSettings.MaxDebuffChance, combatSettings.MinDebuffChance + levelDiff * combatSettings.DebuffChancePerLevel);

                StatusEffect statusEffect = statusSettings.Get(aeffect.EntityId);

                if (statusEffect == null)
                {
                    continue;
                }

                CrawlerSpellEffect spellEffect = new CrawlerSpellEffect()
                {
                    EntityTypeId = aeffect.EntityTypeId,
                    EntityId = aeffect.EntityId,
                    MaxQuantity = 1,
                    MinQuantity = 1,
                    ElementTypeId = statusEffect.ElementTypeId,
                };


                FullEffect fullApplyEffect = new FullEffect()
                {
                    Effect = spellEffect,
                    InitialEffect = true,
                    Chance = effectChance,
                    ElementType = elementSettings.Get(statusEffect.ElementTypeId),
                };

                finalApplyEffects.Add(fullApplyEffect);
            }

            long vulnBits = 0;
            long resistBits = 0;

            foreach (UnitEffect eff in resistEffects)
            {
                resistBits |= (long)(1 << (int)eff.EntityId);
                retval.BonusCount++;
            }

            foreach (UnitEffect eff in vulnEffects)
            {
                vulnBits |= (long)(1 << (int)eff.EntityId);
                retval.BonusCount++;
            }

            bool isGuardian = spells.Any(x => x.EntityTypeId == EntityTypes.CrawlerSpell && defendSpellIds.Contains(x.EntityId));
            if (isGuardian)
            {
                retval.BonusCount++;
            }
            retval.ResistBits = resistBits;
            retval.VulnBits = vulnBits;
            retval.IsGuardian = isGuardian;

            retval.Spells = spells;
            retval.BonusCount += spells.Count;
            retval.ApplyEffects = finalApplyEffects;
            retval.BonusCount += finalApplyEffects.Count;
            retval.ExtraKeywords = extraKeywords;
            retval.SuffixKeyword = suffixKeyword;
            return retval;
        }


        public void AddCombatUnits(PartyData party, InitialCombatGroup initial)
        {

            UnitType unitType = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(initial.UnitTypeId);

            if (unitType == null)
            {
                return;
            }

            if (party.Combat == null)
            {
                return;
            }

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);

            List<CombatGroup> groups = initial.FactionTypeId == FactionTypes.Player ? party.Combat.Allies : party.Combat.Enemies;

            CombatGroup group = groups.FirstOrDefault(x => x.UnitTypeId == unitType.IdKey);

            long combatLevel = party.Combat.Level;

            FullMonsterStats fullStats = GetFullMonsterStats(party, unitType, initial.FactionTypeId, combatLevel, true);

            string namePrefix = "";

            if (fullStats.ExtraKeywords.Count > 0)
            {
                List<UnitKeyword> okNameKeywords = fullStats.ExtraKeywords.Where(x => !unitType.Name.Contains(x.Name) &&
                x != fullStats.SuffixKeyword).ToList();

                if (okNameKeywords.Count > 0)
                {
                    UnitKeyword chosenWord = okNameKeywords[_rand.Next(okNameKeywords.Count)];
                    namePrefix = chosenWord.Name + " ";
                }
            }

            string singularName = namePrefix + unitType.Name;
            string pluralName = namePrefix + unitType.PluralName;
            if (fullStats.SuffixKeyword != null)
            {
                singularName += " " + fullStats.SuffixKeyword.Name;
                pluralName = namePrefix + unitType.Name + " " + fullStats.SuffixKeyword.PluralName;
            }

            if (group == null || !string.IsNullOrEmpty(initial.BossName))
            {
                group = new CombatGroup()
                {
                    Id = party.GetNextId("G"),
                    Range = initial.Range,
                    UnitTypeId = unitType.IdKey,
                    SingularName = singularName,
                    PluralName = pluralName,
                    FactionTypeId = initial.FactionTypeId,
                };

                if (!string.IsNullOrEmpty(initial.BossName))
                {
                    group.SingularName = initial.BossName;
                    group.PluralName = initial.BossName;
                }

                bool didAddGroup = false;
                for (int g = 0; g < groups.Count; g++)
                {
                    if (groups[g].Range > initial.Range)
                    {
                        groups.Insert(g, group);
                        didAddGroup = true;
                        break;
                    }
                }

                if (!didAddGroup)
                {
                    groups.Add(group);
                }
            }

            for (int i = 0; i < initial.Quantity; i++)
            {
                if (group.Units.Count >= combatSettings.MaxGroupSize)
                {
                    break;
                }

                Monster monster = new Monster()
                {
                    Id = party.GetNextId("M"),
                    UnitTypeId = unitType.IdKey,
                    Level = initial.Level,
                    Name = namePrefix + unitType.Name + (i + 1),
                    PortraitName = unitType.Icon,
                    FactionTypeId = initial.FactionTypeId,
                    Spells = fullStats.Spells,
                    ApplyEffects = fullStats.ApplyEffects,
                    IsGuardian = fullStats.IsGuardian,
                    ResistBits = fullStats.ResistBits,
                    VulnBits = fullStats.VulnBits,
                    CombatGroupId = group.Id,
                    ExtraKeywords = fullStats.ExtraKeywords,
                    BonusCount = fullStats.BonusCount,
                };
                _statService.CalcUnitStats(party, monster, true);

                group.Units.Add(monster);

            }

            _dispatcher.Dispatch(new UpdateCombatGroups());
        }

        public bool ReadyForCombat(PartyData party)
        {
            if (party.Combat == null)
            {
                return false;
            }

            foreach (CombatGroup group in party.Combat.Allies)
            {
                if (group.CombatGroupAction != ECombatGroupActions.Fight)
                {
                    continue;
                }
                foreach (CrawlerUnit unit in group.Units)
                {
                    if (unit is PartyMember member)
                    {
                        if (unit.Action == null)
                        {
                            if (!IsDisabled(unit))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public async Task EndCombatRound(PartyData party)
        {

            try
            {
                if (party.Combat == null || !ReadyForCombat(party))
                {
                    _dispatcher.Dispatch(new UpdateCombatGroups());
                    return;
                }

                party.Combat.PlayerActionsRemaining--;

                CrawlerCombatState combat = party.Combat;

                foreach (CombatGroup group in combat.Enemies)
                {
                    group.CombatGroupAction = ECombatGroupActions.None;
                    List<CrawlerUnit> dupeList = new List<CrawlerUnit>(group.Units);
                    foreach (CrawlerUnit unit in dupeList)
                    {
                        unit.Action = null;
                        if (unit.StatusEffects.HasBit(StatusEffects.Dead))
                        {
                            group.Units.Remove(unit);
                            combat.EnemiesKilled.Add(unit);
                        }

                        List<IDisplayEffect> removeEffectList = new List<IDisplayEffect>();
                        foreach (IDisplayEffect effect in unit.Effects)
                        {
                            if (effect.MaxDuration > 0)
                            {
                                effect.DurationLeft--;
                                if (effect.DurationLeft < 0)
                                {
                                    removeEffectList.Add(effect);
                                }
                            }
                        }

                        foreach (IDisplayEffect effect in removeEffectList)
                        {
                            unit.RemoveEffect(effect);
                        }
                    }
                }
                foreach (CombatGroup group in combat.Allies)
                {
                    group.CombatGroupAction = ECombatGroupActions.None;
                    List<CrawlerUnit> dupeList = new List<CrawlerUnit>(group.Units);
                    foreach (CrawlerUnit unit in dupeList)
                    {
                        unit.Action = null;
                        if (unit.StatusEffects.HasBit(StatusEffects.Dead))
                        {
                            if (!(unit is PartyMember member))
                            {
                                group.Units.Remove(unit);
                            }
                        }
                    }
                }

                _dispatcher.Dispatch(new UpdateCombatGroups());
                combat.Enemies = combat.Enemies.Where(x => x.Units.Count > 0).ToList();
                combat.Allies = combat.Allies.Where(x => x.Units.Count > 0).ToList();
                await _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.CombatRound);
                combat.RoundsComplete++;
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "Combat ");
            }
        }

        public void SetInitialActions(PartyData party)
        {
            // Pass 1 defend and hide


            List<long> defenderRoleIds = _gameData.Get<RoleSettings>(_gs.ch).GetData().Where(x => x.Guardian).Select(x => x.IdKey).ToList();

            foreach (CrawlerUnit unit in party.Combat.PartyGroup.Units)
            {
                if (unit.Action == null || unit.Action.IsComplete)
                {
                    continue;
                }

                unit.DefendRank = EDefendRanks.None;

                foreach (UnitRole unitRole in unit.Roles)
                {
                    if (defenderRoleIds.Contains(unitRole.RoleId))
                    {
                        unit.DefendRank = EDefendRanks.Guardian;
                        unit.IsGuardian = true;
                        break;
                    }
                }

                if (unit.Action.CombatActionId == CombatActions.Defend)
                {
                    if (unit.DefendRank == EDefendRanks.Guardian)
                    {
                        unit.DefendRank = EDefendRanks.Taunt;
                    }
                    else
                    {
                        unit.DefendRank = EDefendRanks.Defend;
                    }
                }
                else if (unit.Action.CombatActionId == CombatActions.Hide)
                {
                    unit.HideExtraRange += CrawlerCombatConstants.RangeDelta;
                }
            }

            foreach (CombatGroup cgroup in party.Combat.Allies)
            {
                if (cgroup == party.Combat.PartyGroup)
                {
                    continue;
                }

                foreach (CrawlerUnit unit in cgroup.Units)
                {
                    if (unit.IsGuardian)
                    {
                        unit.DefendRank = EDefendRanks.Guardian;
                    }
                }
            }
        }

        public bool SetMonsterActions(PartyData party)
        {
            if (party.Combat == null || !ReadyForCombat(party) || party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Prepare ||
                party.Combat.PlayerActionsRemaining > 1)
            {
                return false;
            }

            CrawlerCombatState combat = party.Combat;

            List<CrawlerUnit> tauntUnits = new List<CrawlerUnit>();
            List<CrawlerUnit> hiddenUnits = new List<CrawlerUnit>();
            List<CrawlerUnit> allUnits = new List<CrawlerUnit>();
            List<CrawlerUnit> nonGuardianPlayers = new List<CrawlerUnit>();

            foreach (CombatGroup combatGroup in combat.Allies)
            {
                List<CrawlerUnit> okUnits = combatGroup.Units.Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead) &&
                !x.StatusEffects.HasBit(StatusEffects.Possessed)).ToList();

                tauntUnits.AddRange(okUnits.Where(x => x.DefendRank >= EDefendRanks.Guardian || !x.IsPlayer()));
                allUnits.AddRange(okUnits);
                hiddenUnits.AddRange(okUnits.Where(x => x.HideExtraRange > 0));
            }

            nonGuardianPlayers = allUnits.Where(x => x.IsPlayer()).Except(tauntUnits).Except(hiddenUnits).ToList();

            List<CrawlerUnit> monsterUnits = tauntUnits.Where(x => !x.IsPlayer()).ToList();

            if (monsterUnits.Count > 0 && !tauntUnits.Any(x => x.DefendRank == EDefendRanks.Taunt))
            {
                tauntUnits = monsterUnits;
            }

            List<CrawlerUnit> nonHiddenUnits = tauntUnits.Where(x => x.HideExtraRange == 0).ToList();
            if (nonHiddenUnits.Count > 0)
            {
                tauntUnits = nonHiddenUnits;
            }

            foreach (CombatGroup group in combat.Allies)
            {
                if (group != party.Combat.PartyGroup && party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Fight)
                {
                    SelectGroupActions(party, group, new List<CrawlerUnit>(), hiddenUnits, nonGuardianPlayers, combat.Allies, combat.Enemies);
                }
            }

            foreach (CombatGroup group in combat.Enemies)
            {
                SelectGroupActions(party, group, tauntUnits, hiddenUnits, nonGuardianPlayers, combat.Enemies, combat.Allies);
            }

            return true;
        }

        public void RemoveEndOfCombatEffects(PartyData party)
        {
            foreach (PartyMember member in party.Members)
            {
                List<IDisplayEffect> expiredEffects = member.Effects.Where(x => x.EntityTypeId == EntityTypes.StatusEffect &&
                    x.MaxDuration > 0).ToList();

                foreach (IDisplayEffect effect in expiredEffects)
                {
                    member.RemoveEffect(effect);
                }
            }
        }

        public void SelectGroupActions(PartyData party, CombatGroup group,
            List<CrawlerUnit> tauntUnits,
            List<CrawlerUnit> hiddenUnits,
            List<CrawlerUnit> nonGuardianPlayers,
            List<CombatGroup> friends,
            List<CombatGroup> foes)
        {
            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);

            if (group.Range > CrawlerCombatConstants.MinRange && _rand.NextDouble() < combatSettings.GroupAdvanceChance)
            {
                group.CombatGroupAction = ECombatGroupActions.Advance;
            }
            else
            {
                group.CombatGroupAction = ECombatGroupActions.Fight;

                List<CrawlerSpell> summonSpells = new List<CrawlerSpell>();
                List<CrawlerSpell> nonSummonSpells = new List<CrawlerSpell>();


                UnitType groupUnit = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(group.UnitTypeId);

                if (groupUnit != null)
                {
                    List<long> spellIds = groupUnit.Effects.Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell).Select(x => x.EntityId).ToList();
                    IReadOnlyList<CrawlerSpell> currentSpells = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData().Where(x => spellIds.Contains(x.IdKey)).ToList();

                    summonSpells = currentSpells.Where(x => x.Effects.Any(e => e.EntityTypeId == EntityTypes.Unit && e.EntityId > 0)).ToList();
                    nonSummonSpells = currentSpells.Except(summonSpells).ToList();
                }

                foreach (CrawlerUnit unit in group.Units)
                {
                    SelectMonsterAction(party, group, unit, tauntUnits, hiddenUnits, nonGuardianPlayers, friends, foes, summonSpells, nonSummonSpells);
                }
            }
        }

        public void SelectMonsterAction(PartyData party, CombatGroup unitGroup,
            CrawlerUnit unit, List<CrawlerUnit> tauntUnits,
            List<CrawlerUnit> hiddenUnits,
            List<CrawlerUnit> nonGuardianPlayers,
            List<CombatGroup> allyGroups, List<CombatGroup> enemyGroups, List<CrawlerSpell> summonSpells,
            List<CrawlerSpell> nonSummonSpells)
        {
            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);
            CrawlerMonsterSettings monsterSettings = _gameData.Get<CrawlerMonsterSettings>(_gs.ch);
            CrawlerSpellSettings spellSettings = _gameData.Get<CrawlerSpellSettings>(_gs.ch);


            if (party.Combat == null)
            {
                return;
            }

            if (unit.IsPlayer())
            {
                if (!unit.StatusEffects.HasBit(StatusEffects.Possessed))
                {
                    return;
                }
                else
                {
                    List<CombatGroup> temp = allyGroups;
                    allyGroups = enemyGroups;
                    enemyGroups = temp;
                    tauntUnits = new List<CrawlerUnit>();
                    return;
                }
            }

            double roleScalingValue = _roleService.GetRoleScalingLevel(party, unit, RoleScalingTypes.SpellDam);

            nonSummonSpells = nonSummonSpells.Where(x => x.RoleScalingTier <= roleScalingValue).ToList();

            List<CrawlerUnit> targets = new List<CrawlerUnit>();

            if (unit.FactionTypeId != FactionTypes.Player)
            {
                if (hiddenUnits.Count > 0 && _rand.Next() % 100 < unit.Stats.Max(StatTypes.DetectHidden))
                {
                    targets.AddRange(hiddenUnits);
                }
                else if (nonGuardianPlayers.Count > 0 && _rand.Next() % 100 < unit.Stats.Max(StatTypes.SmartTarget))
                {
                    targets.AddRange(nonGuardianPlayers);
                }
                else if (tauntUnits.Count > 0)
                {
                    targets.AddRange(tauntUnits);
                }
            }

            if (targets.Count < 1)
            {
                targets = SelectRandomGroupUnits(enemyGroups);
            }

            UnitAction combatAction = new UnitAction()
            {
                Caster = unit,
                FinalTargets = targets,
            };

            // Only enemy monsters summon in combat
            if (!allyGroups.Contains(party.Combat.PartyGroup) && summonSpells.Count > 0 && _rand.NextDouble() < combatSettings.SummonChance)
            {
                CrawlerSpell spell = summonSpells[_rand.Next(summonSpells.Count)];

                long cost = _crawlerSpellService.GetPowerCost(party, unit, spell);

                long mana = unit.Stats.Get(StatTypes.Mana, StatCategories.Curr);

                if (mana >= cost)
                {
                    combatAction.CombatActionId = CombatActions.Cast;
                    combatAction.Spell = spell;
                    combatAction.FinalTargets = new List<CrawlerUnit>() { unit };
                }
            }

            if (combatAction.Spell == null && nonSummonSpells.Count > 0 && _rand.NextDouble() < combatSettings.CastSpellChance)
            {
                CrawlerSpell spell = nonSummonSpells[_rand.Next(nonSummonSpells.Count)];

                long cost = _crawlerSpellService.GetPowerCost(party, unit, spell);

                long mana = unit.Stats.Get(StatTypes.Mana, StatCategories.Curr);

                if (mana >= cost)
                {
                    combatAction.CombatActionId = CombatActions.Cast;
                    combatAction.Spell = spell;
                    combatAction.FinalTargets = targets;

                    if (!_crawlerSpellService.IsEnemyTarget(spell.TargetTypeId))
                    {
                        if (spell.TargetTypeId == TargetTypes.AllAllies)
                        {
                            combatAction.FinalTargets = new List<CrawlerUnit>();
                            combatAction.FinalTargetGroups = new List<CombatGroup>(allyGroups);
                            foreach (CombatGroup cgroup in allyGroups)
                            {
                                //combatAction.FinalTargets.AddRange(cgroup.Units);
                            }
                        }
                        else
                        {
                            combatAction.FinalTargets = new List<CrawlerUnit>() { unit };
                        }
                    }
                    else
                    {
                        if (spell.TargetTypeId == TargetTypes.OneEnemyGroup)
                        {
                            if (enemyGroups.Count > 0)
                            {
                                CombatGroup egroup = enemyGroups[_rand.Next(enemyGroups.Count)];
                                combatAction.FinalTargetGroups = new List<CombatGroup> { egroup };
                                //combatAction.FinalTargets = new List<CrawlerUnit>(egroup.Units);
                            }
                        }
                        else if (spell.TargetTypeId == TargetTypes.EnemyInEachGroup)
                        {
                            combatAction.FinalTargets = new List<CrawlerUnit>();
                            combatAction.FinalTargetGroups = new List<CombatGroup>(enemyGroups);
                            foreach (CombatGroup egroup in enemyGroups)
                            {
                                //combatAction.FinalTargets.AddRange(egroup.Units);
                            }
                        }
                    }
                }
            }

            // Now attack if we didn't cast a spell.
            if (combatAction.Spell == null)
            {
                combatAction.Spell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).Get(CrawlerSpells.AttackId);
                combatAction.CombatActionId = CombatActions.Attack;
                if (unitGroup.Range > CrawlerCombatConstants.MinRange || !enemyGroups.Any(x => x.Range <= CrawlerCombatConstants.MinRange))
                {
                    combatAction.Spell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).Get(CrawlerSpells.ShootId);
                    combatAction.CombatActionId = CombatActions.Shoot;
                }
            }

            if (combatAction.Spell != null)
            {
                unit.Action = combatAction;
            }
        }

        private List<CrawlerUnit> SelectRandomGroupUnits(List<CombatGroup> groups)
        {
            List<CrawlerUnit> allUnits = new List<CrawlerUnit>();

            groups = groups.Where(x => x.Units.Any(u => !u.StatusEffects.HasBit(StatusEffects.Dead))).ToList();
            if (groups.Count > 0)
            {
                return groups[_rand.Next() % groups.Count].Units;
            }
            return null;
        }

        public UnitAction GetActionFromSpell(PartyData party, CrawlerUnit caster, CrawlerSpell spell,
            List<UnitAction> currentActions = null)
        {

            UnitAction newAction = new UnitAction()
            {
                CombatActionId = spell.CombatActionId,
                Spell = spell,
                Caster = caster,
            };

            if (spell.TargetTypeId == TargetTypes.AllAllies)
            {
                if (party.Combat != null)
                {
                    newAction.FinalTargets = new List<CrawlerUnit>();
                    newAction.FinalTargetGroups = new List<CombatGroup>(party.Combat.Allies);
                }
                else
                {
                    newAction.FinalTargets = new List<CrawlerUnit>(party.GetActiveParty());
                }
            }
            else if (spell.TargetTypeId == TargetTypes.Ally)
            {
                newAction.PossibleTargetUnits = new List<CrawlerUnit>(party.GetActiveParty());
            }
            else if (spell.TargetTypeId == TargetTypes.Self)
            {
                newAction.FinalTargets.Add(caster);
            }
            else if (spell.TargetTypeId == TargetTypes.Special)
            {
                // No targets added here.
            }
            else if (spell.TargetTypeId == TargetTypes.World)
            {
                newAction.FinalTargets.Add(caster);
            }
            else // Target must be some kind of enemies.
            {
                List<CombatGroup> possibleGroups = new List<CombatGroup>();

                long minRange = spell.MinRange;
                long maxRange = spell.MaxRange;
                if (caster.HideExtraRange > 0)
                {
                    maxRange = caster.HideExtraRange + CrawlerCombatConstants.MinRange;
                }

                foreach (CombatGroup group in party.Combat.Enemies)
                {

                    if (group.Range >= minRange && group.Range <= maxRange)
                    {
                        possibleGroups.Add(group);
                    }
                }

                if (possibleGroups.Count < 1)
                {
                    return null;
                }
                else if (possibleGroups.Count > 1)
                {
                    if (spell.TargetTypeId == TargetTypes.AllEnemies || spell.TargetTypeId == TargetTypes.EnemyInEachGroup)
                    {
                        for (int g = 0; g < possibleGroups.Count; g++)
                        {
                            CombatGroup group = possibleGroups[g];

                            foreach (CrawlerUnit crawlerUnit in group.Units)
                            {
                                newAction.FinalTargets.Add(crawlerUnit);
                            }
                        }
                    }
                    else
                    {
                        newAction.PossibleTargetGroups = new List<CombatGroup>(possibleGroups);
                    }

                }
                else if (possibleGroups.Count == 1)
                {
                    newAction.FinalTargets.AddRange(possibleGroups[0].Units.Select(x => x).ToList());
                }
            }

            if (spell.TargetTypeId != TargetTypes.Special &&
                newAction.FinalTargets.Count < 1 && newAction.PossibleTargetUnits.Count < 1 && newAction.PossibleTargetGroups.Count < 1)
            {
                return null;
            }
            UnitAction currAction = null;

            if (currentActions != null)
            {
                currAction = currentActions.FirstOrDefault(x => x.CombatActionId == newAction.CombatActionId);
            }

            if (currAction == null)
            {
                CombatAction combatAction = _gameData.Get<CombatActionSettings>(_gs.ch).Get(newAction.CombatActionId);

                if (combatAction == null)
                {
                    _logService.Info("BadCombatAction " + newAction.Spell.Name + " " + newAction.CombatActionId);
                    return null;
                }

                newAction.Text = combatAction.Name;
                if (combatAction.Name != spell.Name)
                {
                    newAction.Text += ": " + spell.Name;
                }

                if (spell != null)
                {
                    double spellLevel = _roleService.GetSpellScalingLevel(party, caster, spell);
                    newAction.Text += " [Tier: " + spellLevel + "]";
                }

                if (newAction.CombatActionId == CombatActions.Defend)
                {
                    if (caster.DefendRank >= EDefendRanks.Guardian)
                    {
                        newAction.Text += ": (Taunt)";
                    }

                }


            }
            else
            {
                newAction.Text = spell.Name;

                if (spell.CombatActionId == CombatActions.Hide)
                {
                    newAction.Text += "(" + (caster.DefendRank + CrawlerCombatConstants.MinRange) + "')";
                }
            }

            return newAction;
        }

        public List<UnitAction> GetActionsForPlayer(PartyData party, CrawlerUnit unit)
        {
            PartyMember member = unit as PartyMember;

            List<UnitAction> retval = new List<UnitAction>();

            if (IsDisabled(member))
            {
                retval.Add(new UnitAction()
                {
                    CombatActionId = CombatActions.Disabled,
                });
                return retval;
            }

            List<CrawlerSpell> nonCastSpells = _crawlerSpellService.GetNonSpellCombatActionsForMember(party, member);

            foreach (CrawlerSpell spell in nonCastSpells)
            {
                UnitAction newAction = GetActionFromSpell(party, unit, spell, retval);
                if (newAction != null)
                {
                    retval.Add(newAction);
                }
            }

            List<CrawlerSpell> spells = _crawlerSpellService.GetSpellsForMember(party, member);

            if (spells.Count > 0)
            {
                retval.Add(new UnitAction() { Caster = member, CombatActionId = CombatActions.Cast, Text = "Cast" });
            }

            if (party.Combat != null)
            {
                CrawlerSpell prevSpell = spells.FirstOrDefault(x => x.IdKey == member.LastCombatCrawlerSpellId);
                if (prevSpell == null)
                {
                    prevSpell = nonCastSpells.FirstOrDefault(x => x.IdKey == member.LastCombatCrawlerSpellId);
                }

                if (prevSpell != null)
                {

                    UnitAction combatAction = GetActionFromSpell(party, member, prevSpell);

                    if (combatAction != null && combatAction.PossibleTargetGroups.Count > 0)
                    {
                        retval.Add(new UnitAction() { Caster = member, Spell = prevSpell, CombatActionId = CombatActions.Recast });
                    }
                }
            }

            if (retval.Count < 1)
            {
                retval.Add(new UnitAction() { Caster = member, CombatActionId = CombatActions.Disabled });
            }

            return retval;
        }



        private List<long> _disabledBits = null;
        public bool IsDisabled(CrawlerUnit unit)
        {
            if (_disabledBits == null)
            {
                _disabledBits = new List<long>();
                IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

                foreach (StatusEffect eff in effects)
                {
                    if (eff.CombatActionId == CombatActions.Disabled)
                    {
                        _disabledBits.Add(eff.IdKey);
                    }
                }
            }

            foreach (long effId in _disabledBits)
            {
                if (unit.StatusEffects.HasBit(effId))
                {
                    return true;
                }
            }

            return false;
        }

        private Dictionary<long, int> _actionToDisableBits = new Dictionary<long, int>()
        {
            [CombatActions.Attack] = 1 << MapMagics.Peaceful,
            [CombatActions.Shoot] = 1 << MapMagics.Peaceful,
            [CombatActions.Cast] = 1 << MapMagics.NoMagic,
        };

        Dictionary<long, List<long>> _combatActionBlocks = new Dictionary<long, List<long>>();
        public bool IsActionBlocked(PartyData party, CrawlerUnit unit, long combatActionId)
        {

            if (!_combatActionBlocks.ContainsKey(combatActionId))
            {
                _combatActionBlocks[combatActionId] = new List<long>();
                IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

                foreach (StatusEffect eff in effects)
                {
                    if (eff.CombatActionId == combatActionId && eff.Amount >= 100)
                    {
                        _combatActionBlocks[combatActionId].Add(eff.IdKey);
                    }
                }
            }

            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            int disabledBits = _crawlerMapService.GetMagicBits(party.CurrPos.MapId, party.CurrPos.X, party.CurrPos.Z);

            if (disabledBits > 0)
            {
                if (_actionToDisableBits.ContainsKey(combatActionId) &&
                    FlagUtils.IsSet(_actionToDisableBits[combatActionId], disabledBits))
                {
                    return true;
                }
            }

            if (_combatActionBlocks.TryGetValue(combatActionId, out List<long> blockingStatusEffectList))
            {
                foreach (long statusEffectId in blockingStatusEffectList)
                {
                    if (unit.StatusEffects.HasBit(statusEffectId))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        Dictionary<long, List<IdVal>> _combatActionWeakPercents = new Dictionary<long, List<IdVal>>();
        public long GetWeakReductionPercent(CrawlerUnit unit, long combatActionId)
        {
            StatusEffectSettings settings = _gameData.Get<StatusEffectSettings>(_gs.ch);
            IReadOnlyList<StatusEffect> effects = settings.GetData();
            if (!_combatActionWeakPercents.ContainsKey(combatActionId))
            {
                _combatActionWeakPercents[combatActionId] = new List<IdVal>();

                foreach (StatusEffect eff in effects)
                {
                    if (eff.CombatActionId == combatActionId && eff.Amount < 100)
                    {
                        _combatActionWeakPercents[combatActionId].Add(new IdVal() { Id = eff.IdKey, Val = eff.Amount });
                    }
                }
            }

            long weakAmount = 0;
            foreach (IdVal idval in _combatActionWeakPercents[combatActionId])
            {
                if (unit.StatusEffects.HasBit(idval.Id))
                {
                    weakAmount += idval.Val;
                }
            }

            if (unit.StatusEffects.HasBit(StatusEffects.Weak))
            {
                weakAmount += settings.Get(StatusEffects.Weak).Amount;
            }

            return weakAmount;
        }

        public string ShowGroupStatus(CombatGroup group)
        {
            UnitType unitType = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(group.UnitTypeId);

            return group.Units.Count + " " + _infoService.CreateInfoLink(unitType,
                (group.Units.Count == 1 ? group.SingularName : group.PluralName)) +
                (group.Range > CrawlerCombatConstants.MinRange ?
                " (" + group.Range + "')" : "");

        }

        public bool ProccedStatusEffect(CrawlerUnit unit, long statusEffectId)
        {
            if (!unit.StatusEffects.HasBit(statusEffectId))
            {
                return false;
            }

            return _rand.Next(100) < _gameData.Get<StatusEffectSettings>(_gs.ch).Get(statusEffectId).Amount;
        }
    }
}

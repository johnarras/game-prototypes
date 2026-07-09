using Assets.Scripts.Audio.ClientEvents;
using Assets.Scripts.Cameras.Constants;
using Assets.Scripts.Crawler.ClientEvents.CombatEvents;
using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.Items.Services;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Crawler.Shared.Combat.Constants;
using Assets.Scripts.Dungeons.Audio;
using Assets.Scripts.Dungeons.Audio.Constants;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.Combat.Constants;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Combat.Settings;
using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.Info.Services;
using OxDb.SharedGame.Crawler.Items.Entities;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Monsters.Settings;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Options.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Quests.Services;
using OxDb.SharedGame.Crawler.Roles.Services;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.Spells.Entities;
using OxDb.SharedGame.Crawler.Spells.Services;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Stats.Services;
using OxDb.SharedGame.Crawler.TimeOfDay.Constants;
using OxDb.SharedGame.Crawler.TimeOfDay.Services;
using OxDb.SharedGame.Crawler.Upgrades.Constants;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Factions.Constants;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Spells.Constants;
using OxDb.SharedGame.Spells.Interfaces;
using OxDb.SharedGame.Spells.Settings.Elements;
using OxDb.SharedGame.UnitEffects.Constants;
using OxDb.SharedGame.UnitEffects.Settings;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Units.Settings;
using OxDb.SharedGame.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.Combat.Services
{
    public interface ICrawlerCombatService : IInitializable
    {
        Task<bool> StartCombat(PartyData party);
        Task<ECombatStepResults> EndCombatRound(PartyData party, CancellationToken token);
        bool ReadyForCombat(PartyData party);
        bool IsDisabled(CrawlerUnit unit);
        bool IsActionBlocked(PartyData party, CrawlerUnit unit, long combatActionId);
        long GetWeakReductionPercent(CrawlerUnit unit, long combatActionId);
        List<UnitAction> GetActionsForPlayer(PartyData party, CrawlerUnit unit);
        UnitAction GetActionFromSpell(PartyData party, CrawlerUnit unit, CrawlerSpell spell,
            List<UnitAction> currentActions = null, Item castingItem = null);
        void AddCombatUnits(PartyData party, InitialCombatGroup initial);
        void EndCombat(PartyData party);
        string ShowGroupStatus(CombatGroup group);
        int GetMaxGroupSize(PartyData party, long level, double difficulty = 1.0f);
        FullMonsterStats GetFullMonsterStats(PartyData party, UnitType unitType, long factionTypeId, long combatLevel, bool isForCombat);
        bool ProccedStatusEffect(CrawlerUnit unit, long statusEffectId);
        void InitPartyCombatActions(PartyData party);
        bool IsValidEnemyTarget(CrawlerUnit unit);
    }
    public class CrawlerCombatService : ICrawlerCombatService
    {
        private ICrawlerStatService _statService = null;
        private ICrawlerSpellService _crawlerSpellService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
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
        private ICrawlerItemService _crawlerItemService = null;
        private ICrawlerOptionsService _optionsService = null;
        private ICrawlerUpgradeService _crawlerUpgradeService = null;
        private ICameraController _cameraController = null;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public void EndCombat(PartyData party)
        {

            if (party.Combat != null)
            {
                party.Combat.Allies.Clear();
                party.Combat.Enemies.Clear();
                party.Combat.AttackSequence.Clear();
                party.Combat.EnemiesKilled.Clear();
            }

            party.Combat = null;


            IReadOnlyList<StatusEffect> statusEffects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

            foreach (PartyMember member in party.ActiveParty)
            {
                member.CombatActions.Clear();
                member.DoTDamage = 0;
                foreach (StatusEffect effect in statusEffects)
                {
                    if (effect.RemoveAtEndOfCombat)
                    {
                        member.StatusEffects.RemoveBitIndex(effect.IdKey);
                    }
                }
            }

            _crawlerMapService.PlayMapSounds();
            _dispatcher.Dispatch(new UpdateCombatGroups());
        }

        public int GetMaxGroupSize(PartyData party, long level, double difficulty = 1.0f)
        {
            StartCombatSettings startSettings = _gameData.Get<StartCombatSettings>(_gs.ch);

            double maxGroupSize = Math.Min(startSettings.MaxGroupSize, (startSettings.StartMaxGroupSize + difficulty * startSettings.GroupSizeIncreasePerLevel));

            if (!_optionsService.HasOption(party, CrawlerOptions.WholeParty))
            {
                maxGroupSize /= 2;
            }
            return (int)Math.Max(1, maxGroupSize);
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

            party.ItemsUsed.Clear();
            if (initialState.Level < 1)
            {
                initialState.Level = await _worldService.GetMapLevelAtParty(party);
            }

            CrawlerCombatState combatState = new CrawlerCombatState() { Level = initialState.Level };

            List<PartyMember> members = party.ActiveParty;

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            StartCombatSettings startSettings = _gameData.Get<StartCombatSettings>(_gs.ch);

            CrawlerSpellSettings spellSettings = _gameData.Get<CrawlerSpellSettings>(_gs.ch);

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);

            combatState.MaxDebuffTier = combatSettings.DebuffTiersPerUnitLevel * initialState.Level;

            List<InitialCombatGroup> partySummons = new List<InitialCombatGroup>();

            CombatGroup partyGroup = new CombatGroup()
            {
                SingularName = "Player",
                PluralName = "Players",
                Id = party.GetNextId("PG"),
                UnitType = _gameData.Get<UnitTypeSettings>(_gs.ch).GetData().First(),
            };

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

                long maxGroupSize = GetMaxGroupSize(party, level, difficulty);

                int groupCount = CrawlerCombatConstants.MinGroupCount;

                double groupCountIncreaseChance = MathUtil.Clamp(startSettings.BaseGroupCountIncreaseChance,
                    startSettings.BaseGroupCountIncreaseChance + startSettings.GroupCountIncreaseChancePerLevel * difficulty,
                    startSettings.MaxGroupCountIncreaseChance);

                while (_gs.Rand.NextDouble() < groupCountIncreaseChance && groupCount < startSettings.MaxGroupCount)
                {
                    // Make this mult < 1 so it's less liekly to keep adding groups as you add more.
                    groupCountIncreaseChance *= startSettings.GroupCountIncreaseMultPerGroupAdded;
                    groupCount++;
                }

                int maxGroupCount = (int)Math.Min(startSettings.MaxGroupCount, CrawlerCombatConstants.MinGroupCount + (int)(startSettings.MaxGroupCountPerLevel * difficulty));

                groupCount = Math.Min(groupCount, maxGroupCount);

                KillQuestTargetResult killResult = await _questService.GetKillQuestTargets(party, level);


                bool canGetQuestCredit = _questService.CanGetQuestCredit(party, level);

                List<UnitType> chosenUnitTypes = new List<UnitType>();

                if (!canGetQuestCredit && killResult.AllPossibleUnitTypeIds.Count > 0)
                {
                    spawns = spawns.Where(x => !killResult.AllPossibleUnitTypeIds.Contains(x.UnitTypeId)).ToList();
                }

                while (chosenUnitTypes.Count < groupCount && spawns.Count > 0)
                {
                    ZoneUnitSpawn chosenSpawn = null;

                    if (_gs.Rand.NextDouble() > startSettings.SelectRandomUnitForCombatGroupChance)
                    {
                        double chanceSum = spawns.Sum(x => x.Weight);

                        double chanceChosen = _gs.Rand.NextDouble() * chanceSum;

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
                        chosenSpawn = spawns[_gs.Rand.Next() % spawns.Count];
                    }

                    UnitType newUnitType = allUnitTypes.FirstOrDefault(x => x.IdKey == chosenSpawn.UnitTypeId);
                    if (newUnitType != null && newUnitType.MinLevel <= level)
                    {
                        chosenUnitTypes.Add(newUnitType);
                    }
                    spawns.Remove(chosenSpawn);
                }


                if (killResult.CurrentUnits.Count > 0 && !chosenUnitTypes.Contains(killResult.CurrentUnits[0]))
                {
                    if (chosenUnitTypes.Count > 0)
                    {
                        chosenUnitTypes.RemoveAt(0);
                    }
                    chosenUnitTypes.Insert(0, killResult.CurrentUnits[0]);
                }

                int currRange = CrawlerCombatConstants.MinRange;

                for (int u = 0; u < chosenUnitTypes.Count; u++)
                {
                    UnitType unitType = chosenUnitTypes[u];

                    if (currRange < CrawlerCombatConstants.MaxRange - CrawlerCombatConstants.RangeDelta * 2)
                    {
                        if (_gs.Rand.NextDouble() < startSettings.RangeIncreaseChancePerGroup)
                        {
                            currRange += CrawlerCombatConstants.RangeDelta;

                            if (u > 0 && _gs.Rand.NextDouble() < startSettings.RangeIncreaseChancePerGroup)
                            {
                                currRange += CrawlerCombatConstants.RangeDelta;
                            }
                        }
                    }

                    // Second chance to push back if this failed.
                    if (u > 0 && currRange == CrawlerCombatConstants.MinRange)
                    {
                        if (_gs.Rand.NextDouble() < startSettings.RangeIncreaseChancePerGroup)
                        {
                            currRange += CrawlerCombatConstants.RangeDelta;
                        }
                    }

                    if (currRange < unitType.MinRange)
                    {
                        currRange = unitType.MinRange;
                    }

                    long quantity = RandUtils.LongRange(CrawlerCombatConstants.MinGroupSize, maxGroupSize, _gs.Rand);

                    InitialCombatGroup initialGroup = new InitialCombatGroup()
                    {
                        UnitTypeId = unitType.IdKey,
                        Range = currRange,
                        Quantity = quantity,
                        FactionTypeId = FactionTypes.Faction1,
                        Level = combatState.Level,
                    };

                    initialState.CombatGroups.Add(initialGroup);

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
                    Quantity = RandUtils.IntRange(5, 10, _gs.Rand),
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

            if (_optionsService.HasOption(party, CrawlerOptions.MoreMonsters))
            {
                foreach (InitialCombatGroup initialGroup in initialState.CombatGroups)
                {
                    initialGroup.Quantity += RandUtils.LongRange(0, initialGroup.Quantity, _gs.Rand);
                }
            }

            // Now save party so players have to come back and fight the monsters even if they quit.
            await _crawlerService.SaveGame();
            party.Combat = combatState;
            _cameraController.SetSaturation(GraphicsConstants.MinSaturation, false);
            _dispatcher.Dispatch(new SetAmbientSoundCategory(AmbientSoundCategoryNames.Combat));
            party.InitialCombat = null;

            _dispatcher.Dispatch(new PlaySound(CrawlerAudio.CombatAmbient, 0, null, 1, true));

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

            List<Effect> spells = new List<Effect>();
            List<Effect> applyEffects = new List<Effect>();

            List<Effect> resistEffects = new List<Effect>();
            List<Effect> vulnEffects = new List<Effect>();

            long suffixKeywordId = 0;
            UnitKeyword suffixKeyword = null;

            if (_gs.Rand.NextDouble() < monsterSettings.UnitKeywordChance && unitType.Keywords.Count > 0)
            {
                List<long> possibleKeywordIds = unitType.Keywords.Select(x => x.UnitKeywordId).ToList();

                List<UnitKeyword> possibleKeywords = allUnitKeywords.Where(x => possibleKeywordIds.Contains(x.IdKey)).ToList();

                if (possibleKeywords.Count > 0)
                {
                    UnitKeyword chosenKeyword = RandUtils.GetRandomElement(possibleKeywords, _gs.Rand);

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

                if (okMapKeywords.Count > 0 && _gs.Rand.NextDouble() < monsterSettings.MapUnitKeywordChance)
                {
                    CurrentUnitKeyword mapKeyword = okMapKeywords[_gs.Rand.Next(okMapKeywords.Count)];

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
            foreach (Effect aeffect in applyEffects)
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
                    WeaponDamageScale = 1,
                    StatBonusDamageScale = 1,
                    ElementTypeId = statusEffect.ElementTypeId,
                };


                FullEffect fullApplyEffect = new FullEffect()
                {
                    Effect = spellEffect,
                    InitialEffect = true,
                    ProcChance = effectChance,
                    ElementType = elementSettings.Get(statusEffect.ElementTypeId),
                };

                finalApplyEffects.Add(fullApplyEffect);
            }

            long vulnBits = 0;
            long resistBits = 0;

            foreach (Effect eff in resistEffects)
            {
                resistBits |= (long)(1 << (int)eff.EntityId);
                retval.BonusCount++;
            }

            foreach (Effect eff in vulnEffects)
            {
                vulnBits |= (long)(1 << (int)eff.EntityId);
                retval.BonusCount++;
            }

            bool isGuardian = spells.FastAny(x => x.EntityTypeId == EntityTypes.CrawlerSpell && defendSpellIds.Contains(x.EntityId));
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

            StartCombatSettings startCombatSettings = _gameData.Get<StartCombatSettings>(_gs.ch);

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);

            List<CombatGroup> groups = initial.FactionTypeId == FactionTypes.Player ? party.Combat.Allies : party.Combat.Enemies;

            CombatGroup group = groups.FirstOrDefault(x => x.UnitType.IdKey == unitType.IdKey);

            long combatLevel = party.Combat.Level;

            FullMonsterStats fullStats = GetFullMonsterStats(party, unitType, initial.FactionTypeId, combatLevel, true);

            string namePrefix = "";

            if (fullStats.ExtraKeywords.Count > 0)
            {
                List<UnitKeyword> okNameKeywords = fullStats.ExtraKeywords.Where(x => !unitType.Name.Contains(x.Name) &&
                x != fullStats.SuffixKeyword).ToList();

                if (okNameKeywords.Count > 0)
                {
                    UnitKeyword chosenWord = okNameKeywords[_gs.Rand.Next(okNameKeywords.Count)];
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
                    UnitType = unitType,
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

            List<Monster> newMonsters = new List<Monster>();
            for (int i = 0; i < initial.Quantity; i++)
            {
                if (group.Units.Count >= startCombatSettings.MaxGroupSize)
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
                    SummonArgs = initial.SummonArgs,
                };
                _statService.CalcUnitStats(party, monster, true);

                newMonsters.Add(monster);

            }

            group.Units.AddRange(newMonsters);

            _dispatcher.Dispatch(new UpdateCombatGroups());
        }

        public bool ReadyForCombat(PartyData party)
        {
            if (party.Combat == null)
            {
                return false;
            }

            List<CrawlerUnit> notReadyUnits = new List<CrawlerUnit>();
            foreach (CombatGroup group in party.Combat.Allies)
            {
                if (group.CombatGroupAction != ECombatGroupActions.Fight)
                {
                    continue;
                }

                foreach (CrawlerUnit unit in group.Units)
                {
                    if (unit.CombatActions.Count < unit.ActionsThisRound)
                    {
                        if (!IsDisabled(unit))
                        {
                            notReadyUnits.Add(unit);
                        }
                    }
                }
            }

            return notReadyUnits.Count == 0;
        }

        public async Task<ECombatStepResults> EndCombatRound(PartyData party, CancellationToken token)
        {
            await _crawlerSpellService.UpdateAtEndOfCombatRound(party, token);
            try
            {
                if (party.Combat == null || !ReadyForCombat(party))
                {
                    _dispatcher.Dispatch(new UpdateCombatGroups());
                    return ECombatStepResults.Continue;
                }

                CrawlerCombatState combat = party.Combat;

                foreach (CombatGroup group in combat.Enemies)
                {
                    group.CombatGroupAction = ECombatGroupActions.None;
                    List<CrawlerUnit> dupeList = new List<CrawlerUnit>(group.Units);
                    foreach (CrawlerUnit unit in dupeList)
                    {
                        unit.CombatActions.Clear();

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
                        unit.CombatActions.Clear();
                    }
                }

                combat.Enemies = combat.Enemies.Where(x => x.Units.Count > 0).ToList();
                combat.Allies = combat.Allies.Where(x => x.Units.Count > 0).ToList();
                await _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.CombatRound);
                combat.RoundsComplete++;
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "Combat ");
            }
            return ECombatStepResults.Continue;
        }


        public void RemoveEndOfCombatEffects(PartyData party)
        {
            foreach (PartyMember member in party.ActiveParty)
            {
                List<IDisplayEffect> expiredEffects = member.Effects.Where(x => x.EntityTypeId == EntityTypes.StatusEffect &&
                    x.MaxDuration > 0).ToList();

                foreach (IDisplayEffect effect in expiredEffects)
                {
                    member.RemoveEffect(effect);
                }
            }
        }


        public UnitAction GetActionFromSpell(PartyData party, CrawlerUnit caster, CrawlerSpell spell,
            List<UnitAction> currentActions = null, Item item = null)
        {

            UnitAction newAction = new UnitAction()
            {
                CombatActionId = spell.CombatActionId,
                Spell = spell,
                Caster = caster,
                CastingItem = item,
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
                    newAction.FinalTargets = new List<CrawlerUnit>(party.ActiveParty);
                }
            }
            else if (spell.TargetTypeId == TargetTypes.Ally)
            {
                newAction.PossibleTargetUnits = new List<CrawlerUnit>(party.ActiveParty);
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
                    double spellLevel = _roleService.GetSpellScalingLevel(party, caster, spell, true);
                    newAction.Text += " [" + spellLevel + "x]";
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

                List<MemberItemSpell> itemSpells = _crawlerItemService.GetUsableItemsForMember(party, member);

                if (itemSpells.Count > 0)
                {
                    retval.Add(new UnitAction() { Caster = member, CombatActionId = CombatActions.UseItem, Text = "Use Item" });
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
                if (unit.StatusEffects.HasBitIndex(effId))
                {
                    return true;
                }
            }

            return false;
        }

        private Dictionary<long, int> _actionToDisableBits = new Dictionary<long, int>()
        {
            [CombatActions.Cast] = 1 << (MapMagics.NoMagic) | (1 << MapMagics.Silence),
            [CombatActions.UseItem] = (1 << MapMagics.NoMagic),
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

            int disabledBits = _crawlerMapService.GetMagicBits(party.CurrPos.MapId, party.CurrPos.X, party.CurrPos.Z, true);

            if (disabledBits > 0)
            {
                if (_actionToDisableBits.ContainsKey(combatActionId) &&
                    FlagUtils.MatchesAnyBits(_actionToDisableBits[combatActionId], disabledBits))
                {
                    return true;
                }
            }

            if (_combatActionBlocks.TryGetValue(combatActionId, out List<long> blockingStatusEffectList))
            {
                foreach (long statusEffectId in blockingStatusEffectList)
                {
                    if (unit.StatusEffects.HasBitIndex(statusEffectId))
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
                if (unit.StatusEffects.HasBitIndex(idval.Id))
                {
                    weakAmount += idval.Val;
                }
            }

            if (unit.StatusEffects.HasBitIndex(StatusEffects.Weak))
            {
                weakAmount += settings.Get(StatusEffects.Weak).Amount;
            }

            return weakAmount;
        }

        public string ShowGroupStatus(CombatGroup group)
        {
            UnitType unitType = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(group.UnitType.IdKey);

            return group.Units.Count + " " + _infoService.CreateInfoLink(unitType,
                (group.Units.Count == 1 ? group.SingularName : group.PluralName)) +
                (group.Range > CrawlerCombatConstants.MinRange ?
                " (" + group.Range + "')" : "");

        }

        public bool ProccedStatusEffect(CrawlerUnit unit, long statusEffectId)
        {
            if (!unit.StatusEffects.HasBitIndex(statusEffectId))
            {
                return false;
            }

            return _gs.Rand.Next(100) < _gameData.Get<StatusEffectSettings>(_gs.ch).Get(statusEffectId).Amount;
        }

        public void InitPartyCombatActions(PartyData party)
        {

            long totalActions = (long)(1 + _crawlerUpgradeService.GetPartyBonus(party, PartyUpgrades.ActionCount));

            if (!_optionsService.HasOption(party, CrawlerOptions.WholeParty))
            {
                totalActions++;
            }

            foreach (PartyMember member in party.ActiveParty)
            {
                member.CombatActions.Clear();
                if (!IsDisabled(member))
                {
                    member.ActionsThisRound = totalActions;
                }
                else
                {
                    member.ActionsThisRound = 0;
                }
            }
        }

        public bool IsValidEnemyTarget(CrawlerUnit unit)
        {
            return !unit.StatusEffects.HasBitIndex(StatusEffects.Dead) &&
                !unit.StatusEffects.HasBitIndex(StatusEffects.Possessed);
        }
    }
}



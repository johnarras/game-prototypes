using Assets.Scripts.Crawler.MapGen.Helpers;
using Assets.Scripts.Crawler.MapGen.Services;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Quests.ClientEvents;
using Assets.Scripts.FloatingText.ClientEvents;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.Loot.Services;
using OxDb.SharedGame.Crawler.MapGen.Helpers;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Options.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Quests.Constants;
using OxDb.SharedGame.Crawler.Quests.Entities;
using OxDb.SharedGame.Crawler.Quests.Helpers;
using OxDb.SharedGame.Crawler.Quests.Settings;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Upgrades.Constants;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Units.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.SharedGame.Crawler.Quests.Services
{
    public class NPCQuestStatus
    {
        public long NpcTypeId { get; set; }
        public List<FullQuest> AvailableQuests { get; set; } = new List<FullQuest>();
        public List<FullQuest> CurrentQuests { get; set; } = new List<FullQuest>();
        public List<CrawlerQuest> CompletedQuests { get; set; } = new List<CrawlerQuest>();
    }


    public class KillQuestTargetResult
    {
        public List<long> AllPossibleUnitTypeIds { get; set; } = new List<long>();
        public List<UnitType> CurrentUnits { get; set; } = new List<UnitType>();
    }

    public interface ICrawlerQuestService : IInjectable
    {
        ValueTask SetupQuest(PartyData party, CrawlerWorld world, CrawlerMap startMap, MapLink targetMap, CrawlerNpc npc, CrawlerQuestType questType, IRandom rand, CancellationToken token);
        ValueTask AddWorldQuestGivers(PartyData party, CrawlerWorld world, IRandom rand, CancellationToken token);
        ValueTask CompleteQuest(PartyData party, FullQuest fullQuest, CancellationToken token);
        ICrawlerQuestTypeHelper GetHelper(long questTypeId);
        ValueTask AcceptQuest(PartyData party, FullQuest fullQuest, CancellationToken token);
        void DropQuest(PartyData party, FullQuest fullQuest, CancellationToken token);
        ValueTask<List<string>> UpdateAfterCombat(PartyData party, CrawlerCombatState combat, CancellationToken token);
        ValueTask<KillQuestTargetResult> GetKillQuestTargets(PartyData party, long level);
        ValueTask<string> ShowQuestStatus(PartyData party, long crawlerQuestId, bool showFullDescription, bool showCurrentState, bool showNPC);
        ValueTask CheckForCompletedQuests(PartyData party);
        ValueTask GiveExploreQuestCredit(PartyData party, long mapId);
        ValueTask<NPCQuestStatus> GetNpcQuestStatus(PartyData party, CrawlerWorld world, long npcTypeId, MapCellDetail npcDetail, CancellationToken token);
        ValueTask<bool> QuestIsActive(PartyData party, long questId);
        ValueTask<List<CrawlerQuest>> GetQuestsForMap(PartyData party, long mapId);
        bool CanGetQuestCredit(PartyData party, long level);

    }

    public class CrawlerQuestService : ICrawlerQuestService
    {

        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private ICrawlerMapGenService _mapGenService = null;
        private ICrawlerWorldService _worldService = null;
        private IDispatcher _dispatcher = null;
        private ICrawlerService _crawlerService = null;
        private ILootGenService _lootGenService = null;
        private ICrawlerUpgradeService _upgradeService = null;
        private ICrawlerOptionsService _optionsService = null;

        private SetupDictionaryContainer<long, ICrawlerQuestTypeHelper> _questTypeHelpers = new SetupDictionaryContainer<long, ICrawlerQuestTypeHelper>();

        public async ValueTask AddWorldQuestGivers(PartyData party, CrawlerWorld world, IRandom rand, CancellationToken token)
        {
            foreach (CrawlerMap startMap in world.Maps)
            {
                await SetupQuestsForMap(party, world, startMap, rand, token);
            }
        }

        private async ValueTask SetupQuestsForMap(PartyData party, CrawlerWorld world, CrawlerMap startMap, IRandom rand, CancellationToken token)
        {
            CrawlerQuestSettings questSettings = _gameData.Get<CrawlerQuestSettings>(_gs.ch);

            ICrawlerMapGenHelper mapGenHelper = _mapGenService.GetGenHelper(startMap.CrawlerMapTypeId);

            List<MapCellDetail> npcDetails = startMap.Details.Where(x => x.EntityTypeId == EntityTypes.Npc).ToList();

            foreach (MapCellDetail npcDetail in npcDetails)
            {
                CrawlerNpc npc = world.GetNpc(npcDetail.EntityId);

                if (npc == null)
                {
                    continue;
                }
                NpcQuestMaps maps = mapGenHelper.GetQuestMapsForNpc(party, world, startMap, npcDetail, rand);

                List<MapLink> allMaps = new List<MapLink>();

                allMaps.AddRange(maps.PrimaryMaps.OrderBy(x => HashUtils.NewGuid()));
                allMaps.AddRange(maps.SecondaryMaps.OrderBy(x => HashUtils.NewGuid()));

                allMaps = allMaps.Where(x => x.Map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon).ToList();

                if (allMaps.Count < 1)
                {
                    continue;
                }

                int questCount = questSettings.MinQuestsPerNpc;

                while (rand.NextDouble() < questSettings.ExtraQuestChance && questCount < questSettings.MaxQuestsPerNpc)
                {
                    questCount++;
                }

                if (questCount > 2 * allMaps.Count * 2)
                {
                    questCount = allMaps.Count * 2;
                }

                if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld))
                {
                    questCount = questSettings.SingleDungeonNpcQuestCount;
                }

                while (GetAllQuestsForNpc(party, world, npc.IdKey).Count < questCount)
                {
                    MapLink targetMap = allMaps[rand.Next(allMaps.Count)];

                    CrawlerQuestType questType = RandUtils.GetRandomElement(questSettings.GetData(), rand);

                    await SetupQuest(party, world, startMap, targetMap, npc, questType, rand, token);
                }
            }
        }


        public ICrawlerQuestTypeHelper GetHelper(long questTypeId)
        {
            if (_questTypeHelpers.TryGetValue(questTypeId, out var helper))
            {
                return helper;
            }
            return null;
        }

        public async ValueTask SetupQuest(PartyData party, CrawlerWorld world, CrawlerMap startMap, MapLink targetMap, CrawlerNpc npc,
            CrawlerQuestType questType, IRandom rand, CancellationToken token)
        {
            ICrawlerQuestTypeHelper helper = GetHelper(questType.IdKey);
            if (helper != null)
            {
                await helper.SetupQuest(party, world, startMap, targetMap, npc, questType, rand, token);
            }
        }

        private void ShowCompleteQuestError(string message)
        {
            _dispatcher.Dispatch(new ShowFloatingText(message, EFloatingTextArt.Error));
        }

        public async ValueTask CompleteQuest(PartyData party, FullQuest fullQuest, CancellationToken token)
        {
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            if (world == null)
            {
                ShowCompleteQuestError("Missing world");
                return;
            }

            if (party.CompletedQuests.HasBitIndex(fullQuest.Quest.IdKey))
            {
                ShowCompleteQuestError("You already completed this quest.");
                return;
            }

            PartyQuest partyQuest = fullQuest.Progress;

            if (partyQuest == null)
            {
                ShowCompleteQuestError("You aren't on this quest!");
                return;
            }

            if (partyQuest.CurrQuantity < fullQuest.Quest.Quantity)
            {
                ShowCompleteQuestError("You aren't finished yet!");
                return;
            }

            CrawlerQuestSettings questSettings = _gameData.Get<CrawlerQuestSettings>(_gs.ch);

            ICrawlerQuestTypeHelper helper = GetHelper(fullQuest.Quest.CrawlerQuestTypeId);

            if (helper == null)
            {
                ShowCompleteQuestError("Unknown Quest Type");
                party.Quests.Remove(partyQuest);
                return;
            }

            CrawlerQuestType questType = questSettings.Get(fullQuest.Quest.CrawlerQuestTypeId);

            long levelAtParty = await _worldService.GetMapLevelAtParty(party);

            int partySize = party.ActiveParty.Count;

            LootGenData lootGenData = await _lootGenService.CreateLootGenData(party, questSettings.ExpLootMult, questSettings.GoldLootMult, questSettings.ItemLootMult, "You Completed a Quest!", ECrawlerStates.NpcMain, fullQuest.NpcDetail);


            lootGenData.ItemCount += (int)RandUtils.IntRange(1, (int)Math.Ceiling(questSettings.ItemLootMult), _gs.Rand);
            party.Quests.Remove(partyQuest);
            party.CompletedQuests.SetBitIndex(fullQuest.Quest.IdKey);


            NewUpgradePointsResult questCompleteResult = _upgradeService.GetNewPartyUpgradePoints(party, levelAtParty, UpgradeReasons.CompleteQuest, "");

            if (questCompleteResult.TotalUpgradePoints > 0)
            {
                lootGenData.TopMessages.Add("+" + questCompleteResult.TotalUpgradePoints + "Upgrade Points!");
            }

            _dispatcher.Dispatch(new UpdateQuestUI());
            _crawlerService.ChangeState(ECrawlerStates.GiveLoot, token, lootGenData);

            if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {

            }

        }

        public async ValueTask AcceptQuest(PartyData party, FullQuest fullQuest, CancellationToken token)
        {
            PartyQuest currQuest = party.Quests.FirstOrDefault(x => x.CrawlerQuestId == fullQuest.Quest.IdKey);

            if (currQuest != null)
            {
                return;
            }

            currQuest = new PartyQuest() { CrawlerQuestId = fullQuest.Quest.IdKey };
            party.Quests.Add(currQuest);


            await CheckForCompletedQuests(party);
            _dispatcher.Dispatch(new UpdateQuestUI());
        }

        public async ValueTask CheckForCompletedQuests(PartyData party)
        {
            bool didCompleteAQuest = false;
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            foreach (PartyQuest partyQuest in party.Quests)
            {
                CrawlerQuest quest = world.GetQuest(partyQuest.CrawlerQuestId);

                if (partyQuest.CurrQuantity >= quest.Quantity)
                {
                    continue;
                }

                if (quest.CrawlerQuestTypeId == CrawlerQuestTypes.ExploreMap)
                {
                    List<long> okMapIds = world.Maps.Where(x => x.BaseCrawlerMapId == quest.TargetEntityId).Select(x => x.IdKey).ToList();

                    foreach (long okMapId in okMapIds)
                    {
                        if (party.CompletedMaps.HasBitIndex(okMapId))
                        {
                            partyQuest.CurrQuantity = quest.Quantity;
                            break;
                        }
                    }
                }
            }

            if (didCompleteAQuest)
            {
                _dispatcher.Dispatch(new UpdateQuestUI());
            }
        }

        public void DropQuest(PartyData party, FullQuest fullQuest, CancellationToken token)
        {
            PartyQuest partyQuest = party.Quests.FirstOrDefault(x => x.CrawlerQuestId == fullQuest.Quest.IdKey);

            if (partyQuest == null)
            {
                return;
            }

            party.Quests.Remove(partyQuest);
            _dispatcher.Dispatch(new UpdateQuestUI());
        }

        public async ValueTask<List<string>> UpdateAfterCombat(PartyData party, CrawlerCombatState combat, CancellationToken token)
        {

            List<CrawlerUnit> killedUnits = combat.EnemiesKilled;

            List<string> retval = new List<string>();
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);
            List<CrawlerQuest> allQuests = await GetQuestsForMap(party, party.CurrPos.MapId);

            CrawlerQuestSettings questSettings = _gameData.Get<CrawlerQuestSettings>(_gs.ch);

            if (allQuests.Count < 1)
            {
                return retval;
            }

            if (!CanGetQuestCredit(party, combat.Level))
            {
                return retval;
            }

            // Do kill quests first.

            List<CrawlerQuest> killQuests = allQuests.Where(x => x.CrawlerQuestTypeId == CrawlerQuestTypes.KillMonsters).ToList();

            if (killQuests.Count > 0)
            {
                Dictionary<long, int> unitQuantities = null;
                foreach (CrawlerQuest killQuest in killQuests)
                {
                    if (party.CompletedQuests.HasBitIndex(killQuest.IdKey))
                    {
                        continue;
                    }

                    PartyQuest partyQuest = party.Quests.FirstOrDefault(x => x.CrawlerQuestId == killQuest.IdKey);

                    if (partyQuest == null || partyQuest.CurrQuantity >= killQuest.Quantity)
                    {
                        continue;
                    }

                    // Don't do this until and unless we need to.
                    if (unitQuantities == null)
                    {
                        unitQuantities = killedUnits.GroupBy(x => x.UnitTypeId).ToDictionary(g => g.Key, g => g.Count());
                    }

                    UnitType unitType = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(killQuest.TargetEntityId);

                    if (unitQuantities.ContainsKey(killQuest.TargetEntityId))
                    {
                        if (partyQuest.CurrQuantity < killQuest.Quantity)
                        {
                            long newQuantity = Math.Min(killQuest.Quantity - partyQuest.CurrQuantity, unitQuantities[killQuest.TargetEntityId]);

                            partyQuest.CurrQuantity += newQuantity;

                            retval.Add($"+" + newQuantity + " " +
                                await ShowQuestStatus(party, killQuest.IdKey, false, true, false));
                            _dispatcher.Dispatch(new UpdateQuestUI());
                        }
                    }
                }
            }

            List<CrawlerQuest> startItemQuests = allQuests.Where(x => x.CrawlerQuestTypeId == CrawlerQuestTypes.LootItems).OrderBy(x => HashUtils.NewGuid()).ToList();

            List<CrawlerQuest> finalItemQuests = new List<CrawlerQuest>();
            foreach (CrawlerQuest itemQuest in startItemQuests)
            {

                if (party.CompletedQuests.HasBitIndex(itemQuest.IdKey))
                {
                    continue;
                }

                PartyQuest partyQuest = party.Quests.FirstOrDefault(x => x.CrawlerQuestId == itemQuest.IdKey);

                if (partyQuest == null || partyQuest.CurrQuantity >= itemQuest.Quantity)
                {
                    continue;
                }

                finalItemQuests.Add(itemQuest);
            }

            if (finalItemQuests.Count > 0)
            {
                double lootChance = questSettings.ItemDropChance * (1 + party.FailedItemQuestTimes);

                long totalQuantity = finalItemQuests.Sum(x => x.Quantity);

                int lootCheckQuantity = killedUnits.Count;

                while (lootCheckQuantity > 10)
                {
                    lootCheckQuantity /= 2;
                    lootChance *= 2;
                }

                if (lootChance > 0.50f)
                {
                    lootChance = 0.50f;
                }

                Dictionary<long, int> quantities = new Dictionary<long, int>();

                foreach (CrawlerQuest itemQuest in finalItemQuests)
                {
                    quantities[itemQuest.IdKey] = 0;
                }

                for (int i = 0; i < lootCheckQuantity; i++)
                {
                    if (_gs.Rand.NextDouble() < lootChance)
                    {
                        long indexChosen = RandUtils.LongRange(0, totalQuantity, _gs.Rand);

                        for (int q = 0; q < finalItemQuests.Count; q++)
                        {
                            indexChosen -= finalItemQuests[q].Quantity;

                            if (indexChosen <= 0)
                            {
                                quantities[finalItemQuests[q].IdKey]++;
                                break;
                            }
                        }
                    }
                }

                long totalFound = quantities.Values.Sum();

                if (totalFound < 1)
                {
                    party.FailedItemQuestTimes++;
                }
                else
                {
                    party.FailedItemQuestTimes = 0;
                }

                foreach (CrawlerQuest quest in finalItemQuests)
                {
                    PartyQuest partyQuest = party.Quests.FirstOrDefault(x => x.CrawlerQuestId == quest.IdKey);
                    long newQuantity = Math.Min(quantities[quest.IdKey], quest.Quantity - partyQuest.CurrQuantity);

                    if (newQuantity > 0)
                    {
                        partyQuest.CurrQuantity += newQuantity;

                        retval.Add($"+" + newQuantity + " " +
                            (newQuantity == 1 ? quest.TargetSingularName : quest.TargetPluralName) +
                            await ShowQuestStatus(party, quest.IdKey, false, true, false));
                        _dispatcher.Dispatch(new UpdateQuestUI());
                    }
                }
            }

            return retval;
        }

        public async ValueTask<KillQuestTargetResult> GetKillQuestTargets(PartyData party, long level)
        {
            KillQuestTargetResult result = new KillQuestTargetResult();

            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            List<CrawlerQuest> currentQuests = await GetQuestsForMap(party, party.CurrPos.MapId);

            if (currentQuests.Count < 1)
            {
                return result;
            }

            bool canGetQuestCredit = CanGetQuestCredit(party, level);

            CrawlerQuestSettings questSettings = _gameData.Get<CrawlerQuestSettings>(_gs.ch);

            foreach (PartyQuest pq in party.Quests)
            {
                CrawlerQuest quest = currentQuests.FirstOrDefault(x => x.IdKey == pq.CrawlerQuestId);

                if (quest == null)
                {
                    continue;
                }

                if (pq.CurrQuantity >= quest.Quantity)
                {
                    continue;
                }

                if (quest.CrawlerQuestTypeId != CrawlerQuestTypes.KillMonsters)
                {
                    continue;
                }

                UnitType utype = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(quest.TargetEntityId);

                if (utype != null)
                {
                    result.AllPossibleUnitTypeIds.Add(utype.IdKey);
                    if (!canGetQuestCredit || _gs.Rand.NextDouble() > questSettings.ForceUnitInCombatChance * (1 + party.FailedKillQuestTimes))
                    {
                        continue;
                    }
                    else
                    {
                        result.CurrentUnits.Add(utype);
                    }
                }
            }

            if (result.CurrentUnits.Count < 1)
            {
                party.FailedKillQuestTimes++;
            }
            else
            {
                result.CurrentUnits = result.CurrentUnits.OrderBy(x => Guid.NewGuid()).ThenBy(x => x.MinRange).ToList();
                party.FailedKillQuestTimes = 0;
            }
            return result;
        }

        public async ValueTask<string> ShowQuestStatus(PartyData party, long currentQuestId, bool showFullDescription, bool showCurrentState, bool showNPC)
        {
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            CrawlerQuest quest = world.GetQuest(currentQuestId);

            if (quest == null)
            {
                return "Unknown Quest";
            }

            if (_questTypeHelpers.TryGetValue(quest.CrawlerQuestTypeId, out ICrawlerQuestTypeHelper helper))
            {
                return await helper.ShowQuestStatus(party, currentQuestId, showFullDescription, showCurrentState, showNPC);
            }

            return "Unknown Quest Type";
        }

        public async ValueTask GiveExploreQuestCredit(PartyData party, long mapId)
        {
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);


            CrawlerMap map = world.GetMap(mapId);

            if (map == null)
            {
                return;
            }

            foreach (CrawlerQuest quest in world.Quests)
            {
                if (quest.CrawlerQuestTypeId == CrawlerQuestTypes.ExploreMap)
                {
                    quest.TargetEntityId = quest.CrawlerMapId;
                }
            }

            List<CrawlerQuest> quests = world.Quests.Where(x => x.CrawlerQuestTypeId == CrawlerQuestTypes.ExploreMap &&
            x.TargetEntityId == map.BaseCrawlerMapId).ToList();

            foreach (CrawlerQuest quest in quests)
            {
                PartyQuest pq = party.Quests.FirstOrDefault(x => x.CrawlerQuestId == quest.IdKey);

                if (pq == null)
                {
                    continue;
                }

                pq.CurrQuantity = quest.Quantity;
                _dispatcher.Dispatch(new UpdateQuestUI());
            }
        }

        protected List<CrawlerQuest> GetAllQuestsForNpc(PartyData party, CrawlerWorld world, long npcId)
        {

            List<CrawlerQuest> startQuests = world.Quests.Where(x => x.StartCrawlerNpcId == npcId).ToList();
            List<CrawlerQuest> endQuests = world.Quests.Where(x => x.EndCrawlerNpcId == npcId).ToList();
            List<CrawlerQuest> allQuests = startQuests.Concat(endQuests).Distinct().OrderBy(x => x.IdKey).ToList();
            return allQuests;
        }

        public async ValueTask<NPCQuestStatus> GetNpcQuestStatus(PartyData party, CrawlerWorld world, long npcId, MapCellDetail currNpcDetail, CancellationToken token)
        {

            List<CrawlerQuest> allQuests = GetAllQuestsForNpc(party, world, npcId);

            List<FullQuest> availableQuests = new List<FullQuest>();

            List<FullQuest> currentQuests = new List<FullQuest>();

            List<CrawlerQuest> completedQuests = new List<CrawlerQuest>();

            CrawlerQuestSettings questSettings = _gameData.Get<CrawlerQuestSettings>(_gs.ch);
            if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                foreach (CrawlerQuest quest in allQuests)
                {
                    if (party.CompletedQuests.HasBitIndex(quest.IdKey))
                    {
                        completedQuests.Add(quest);
                        continue;
                    }
                }

                foreach (CrawlerQuest completedQuest in completedQuests)
                {
                    world.Quests.Remove(completedQuest);
                    party.Quests = party.Quests.Where(x => x.CrawlerQuestId != completedQuest.IdKey).ToList();
                    party.CompletedQuests.RemoveBitIndex(completedQuest.IdKey);
                }

                int totalQuests = availableQuests.Count + currentQuests.Count;

                int quantityToAdd = questSettings.SingleDungeonNpcQuestCount - totalQuests;

                if (quantityToAdd > 0)
                {
                    CrawlerMap cityMap = world.GetMap(1);

                    int startQuestCount = world.Quests.Count;
                    await SetupQuestsForMap(party, world, cityMap, _gs.Rand, token);
                    int endQuestCount = world.Quests.Count;

                    if (endQuestCount > startQuestCount)
                    {
                        await _worldService.SaveWorld(world);
                    }
                }
            }


            completedQuests.Clear();
            foreach (CrawlerQuest quest in allQuests)
            {
                if (party.CompletedQuests.HasBitIndex(quest.IdKey))
                {
                    completedQuests.Add(quest);
                    continue;
                }

                PartyQuest partyQuest = party.Quests.FirstOrDefault(x => x.CrawlerQuestId == quest.IdKey);

                if (partyQuest == null && quest.StartCrawlerNpcId == npcId)
                {
                    availableQuests.Add(new FullQuest() { Quest = quest, ReturnState = ECrawlerStates.NpcMain, NpcDetail = currNpcDetail });
                }
                else if (partyQuest != null && quest.EndCrawlerNpcId == npcId)
                {
                    currentQuests.Add(new FullQuest()
                    {
                        Quest = quest,
                        Progress = partyQuest,
                        NpcDetail = currNpcDetail,
                        ReturnState = ECrawlerStates.NpcMain,
                    });
                }
            }



            NPCQuestStatus questStatus = new NPCQuestStatus()
            {
                AvailableQuests = availableQuests,
                CurrentQuests = currentQuests,
                NpcTypeId = npcId,
            };

            return questStatus;
        }

        public async ValueTask<bool> QuestIsActive(PartyData party, long questId)
        {
            if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                return true;
            }
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            List<CrawlerQuest> quests = await GetQuestsForMap(party, party.CurrPos.MapId);

            return quests.FastAny(x => x.IdKey == questId);

        }

        public async ValueTask<List<CrawlerQuest>> GetQuestsForMap(PartyData party, long mapId)
        {
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            CrawlerMap map = world.GetMap(mapId);

            if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld) && map != null && map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon)
            {
                return world.Quests.ToList();
            }

            List<CrawlerQuest> list = world.GetQuestsForMap(mapId);

            return list;

        }

        public bool CanGetQuestCredit(PartyData party, long level)
        {

            CrawlerQuestSettings questSettings = _gameData.Get<CrawlerQuestSettings>(_gs.ch);
            if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                if (party.MaxLevelEntered - level > questSettings.SingleDungeonMaxLevelGapForCredit)
                {
                    return false;
                }
            }

            return true;
        }
    }
}



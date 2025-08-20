using Assets.Scripts.Crawler.Maps.Services.GenerateMaps;
using Assets.Scripts.Crawler.Quests.ClientEvents;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.Crawlers.Services;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.MapGen.Helpers;
using Genrpg.Shared.Crawler.MapGen.Services;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Quests.Constants;
using Genrpg.Shared.Crawler.Quests.Entities;
using Genrpg.Shared.Crawler.Quests.Helpers;
using Genrpg.Shared.Crawler.Quests.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Upgrades.Constants;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Genrpg.Shared.Crawler.Quests.Services
{
    public interface ICrawlerQuestService : IInjectable
    {
        Awaitable SetupQuest(PartyData party, CrawlerWorld world, CrawlerMap startMap, MapLink targetMap, CrawlerNpc npc, CrawlerQuestType questType, IRandom rand, CancellationToken token);
        Awaitable AddWorldQuestGivers(PartyData party, CrawlerWorld world, IRandom rand, CancellationToken token);
        Awaitable CompleteQuest(PartyData party, FullQuest fullQuest, CancellationToken token);
        ICrawlerQuestTypeHelper GetHelper(long questTypeId);
        Awaitable AcceptQuest(PartyData party, FullQuest fullQuest, CancellationToken token);
        void DropQuest(PartyData party, FullQuest fullQuest, CancellationToken token);
        Awaitable<List<string>> UpdateAfterCombat(PartyData party, List<CrawlerUnit> killedUnits, CancellationToken token);
        Awaitable<List<UnitType>> GetKillQuestTargets(PartyData party);
        Awaitable<string> ShowQuestStatus(PartyData party, long crawlerQuestId, bool showFullDescription, bool showCurrentState, bool showNPC);
        Awaitable CheckForCompletedQuests(PartyData party);
        Awaitable GiveExploreQuestCredit(PartyData party, long mapId);
    }

    public class CrawlerQuestService : ICrawlerQuestService
    {

        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private ICrawlerMapGenService _mapGenService = null;
        private ICrawlerWorldService _worldService = null;
        private IDispatcher _dispatcher = null;
        private ICrawlerService _crawlerService = null;
        private IClientRandom _rand = null;
        private ILootGenService _lootGenService = null;
        private ICrawlerUpgradeService _upgradeService = null;

        private SetupDictionaryContainer<long, ICrawlerQuestTypeHelper> _questTypeHelpers = new SetupDictionaryContainer<long, ICrawlerQuestTypeHelper>();

        public async Awaitable AddWorldQuestGivers(PartyData party, CrawlerWorld world, IRandom rand, CancellationToken token)
        {
            CrawlerQuestSettings questSettings = _gameData.Get<CrawlerQuestSettings>(_gs.ch);

            foreach (CrawlerMap startMap in world.Maps)
            {
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

                    allMaps.AddRange(maps.PrimaryMaps.OrderBy(x => HashUtils.NewUUId()));
                    allMaps.AddRange(maps.SecondaryMaps.OrderBy(x => HashUtils.NewUUId()));

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

                    for (int q = 0; q < questCount; q++)
                    {
                        MapLink targetMap = allMaps[q % allMaps.Count];

                        CrawlerQuestType questType = RandomUtils.GetRandomElement(questSettings.GetData(), rand);

                        await SetupQuest(party, world, startMap, targetMap, npc, questType, rand, token);

                    }
                }
            }

            await Task.CompletedTask;
        }


        public ICrawlerQuestTypeHelper GetHelper(long questTypeId)
        {
            if (_questTypeHelpers.TryGetValue(questTypeId, out var helper))
            {
                return helper;
            }
            return null;
        }

        public async Awaitable SetupQuest(PartyData party, CrawlerWorld world, CrawlerMap startMap, MapLink targetMap, CrawlerNpc npc,
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

        public async Awaitable CompleteQuest(PartyData party, FullQuest fullQuest, CancellationToken token)
        {
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            if (world == null)
            {
                ShowCompleteQuestError("Missing world");
                return;
            }

            if (party.CompletedQuests.HasBit(fullQuest.Quest.IdKey))
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

            int levelAtParty = await _worldService.GetMapLevelAtParty(party);

            LootGenData lootGenData = await _lootGenService.CreateLootGenData(party, questSettings.ExpLootMult, questSettings.GoldLootMult, questSettings.ItemLootMult, "You Completed a Quest!", ECrawlerStates.NpcMain, fullQuest.NpcDetail);

            party.Quests.Remove(partyQuest);
            party.CompletedQuests.SetBit(fullQuest.Quest.IdKey);


            NewUpgradePointsResult questCompleteResult = _upgradeService.GetNewPartyUpgradePoints(party, levelAtParty, UpgradeReasons.CompleteQuest, "");

            if (questCompleteResult.TotalUpgradePoints > 0)
            {
                lootGenData.TopMessages.Add("+" + questCompleteResult.TotalUpgradePoints + "Upgrade Points!");
            }

            _dispatcher.Dispatch(new UpdateQuestUI());
            _crawlerService.ChangeState(ECrawlerStates.GiveLoot, token, lootGenData);

        }

        public async Awaitable AcceptQuest(PartyData party, FullQuest fullQuest, CancellationToken token)
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

        public async Awaitable CheckForCompletedQuests(PartyData party)
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
                        if (party.CompletedMaps.HasBit(okMapId))
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

        public async Awaitable<List<string>> UpdateAfterCombat(PartyData party, List<CrawlerUnit> killedUnits, CancellationToken token)
        {

            List<string> retval = new List<string>();
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);
            List<CrawlerQuest> allQuests = world.GetQuestsForMap(party.CurrPos.MapId).ToList();

            CrawlerQuestSettings questSettings = _gameData.Get<CrawlerQuestSettings>(_gs.ch);

            if (allQuests.Count < 1)
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
                    if (party.CompletedQuests.HasBit(killQuest.IdKey))
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

            List<CrawlerQuest> startItemQuests = allQuests.Where(x => x.CrawlerQuestTypeId == CrawlerQuestTypes.LootItems).OrderBy(x => HashUtils.NewUUId()).ToList();

            List<CrawlerQuest> finalItemQuests = new List<CrawlerQuest>();
            foreach (CrawlerQuest itemQuest in startItemQuests)
            {

                if (party.CompletedQuests.HasBit(itemQuest.IdKey))
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
                    if (_rand.NextDouble() < lootChance)
                    {
                        long indexChosen = MathUtils.LongRange(0, totalQuantity, _rand);

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

        public async Awaitable<List<UnitType>> GetKillQuestTargets(PartyData party)
        {
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            List<CrawlerQuest> currentQuests = world.GetQuestsForMap(party.CurrPos.MapId);

            if (currentQuests.Count < 1)
            {
                return new List<UnitType>();
            }

            CrawlerQuestSettings questSettings = _gameData.Get<CrawlerQuestSettings>(_gs.ch);

            List<UnitType> retval = new List<UnitType>();

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


                    if (_rand.NextDouble() > questSettings.ForceUnitInCombatChance * (1 + party.FailedKillQuestTimes))
                    {
                        continue;
                    }

                    else
                    {
                        retval.Add(utype);
                    }
                }
            }

            if (retval.Count < 1)
            {
                party.FailedKillQuestTimes++;
            }
            else
            {
                party.FailedKillQuestTimes = 0;
            }
            return retval;
        }

        public async Awaitable<string> ShowQuestStatus(PartyData party, long currentQuestId, bool showFullDescription, bool showCurrentState, bool showNPC)
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

        public async Awaitable GiveExploreQuestCredit(PartyData party, long mapId)
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
    }
}

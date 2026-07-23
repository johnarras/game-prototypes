using OxDb.Client.Crawler.Maps.Entities;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Crawler.Tavern.Services
{
    public interface ITavernService : IInjectable
    {
        string GetRumor(PartyData party, CrawlerWorld world);
    }

    public class TavernService : ITavernService
    {
        private IClientGameState _gs = null;
        private ICrawlerMapService _mapService = null;

        public string GetRumor(PartyData party, CrawlerWorld world)
        {
            if (world.QuestItems.Count < 1)
            {
                return "Lots of scary monsters out there.";
            }

            bool forceQuestItem = false;
            WorldQuestItem questItem = world.QuestItems[_gs.Rand.Next(world.QuestItems.Count)];

            CrawlerMap partyMap = world.GetMap(party.CurrPos.MapId);

            if (partyMap != null)
            {
                List<MapCellDetail> exits = partyMap.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

                List<CrawlerMap> dungeonExits = new List<CrawlerMap>();

                foreach (MapCellDetail detail in exits)
                {
                    CrawlerMap dmap = world.GetMap(detail.EntityId);
                    if (dmap != null && dmap.CrawlerMapTypeId == CrawlerMapTypes.Dungeon &&
                        dmap.MapQuestItemId > 0)
                    {
                        dungeonExits.Add(dmap);
                    }
                }


                if (dungeonExits.Count > 0)
                {
                    CrawlerMap finalMap = dungeonExits[_gs.Rand.Next() % dungeonExits.Count];

                    questItem = world.QuestItems.FirstOrDefault(x => x.IdKey == finalMap.IdKey);

                    if (_gs.Rand.NextDouble() < 0.80f)
                    {
                        forceQuestItem = true;
                    }
                }
            }

            if (!forceQuestItem && _gs.Rand.NextDouble() < 0.35f)
            {
                List<CrawlerMap> subMaps = world.Maps.Where(x => x.CrawlerMapTypeId == CrawlerMapTypes.Dungeon).ToList();

                CrawlerMap targetMap = subMaps[_gs.Rand.Next() % subMaps.Count];

                EntranceMapData entranceMap = _mapService.GetEntranceMap(party, world, targetMap.IdKey);

                if (entranceMap != null && entranceMap.IsValid())
                {
                    return targetMap.Name + "\ncan be found within\n" + entranceMap.EntranceMapName;
                }

                List<MapCellDetail> details = targetMap.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

                List<CrawlerMap> exitMaps = new List<CrawlerMap>();

                foreach (MapCellDetail detail in details)
                {
                    CrawlerMap otherMap = world.GetMap(detail.EntityId);

                    if (otherMap != null && otherMap.CrawlerMapTypeId != CrawlerMapTypes.Dungeon)
                    {
                        exitMaps.Add(otherMap);
                    }
                }

                if (exitMaps.Count > 0)
                {
                    CrawlerMap exitMap = exitMaps[_gs.Rand.Next(exitMaps.Count)];

                    EntranceMapData entranceData = _mapService.GetEntranceMap(party, world, exitMap.IdKey);

                    if (entranceData != null && entranceData.IsValid())
                    {

                    }
                    return targetMap.Name + "\ncan be found within\n" + entranceData.EntranceMapName;
                }
            }

            if (questItem != null)
            {
                CrawlerMap foundMap = world.GetMap(questItem.FoundInMapId);

                CrawlerMap unlockMap = world.GetMap(questItem.UnlocksMapId);

                if (foundMap != null && unlockMap != null)
                {
                    return questItem.Name + " that lies within " + foundMap.Name
                        + " unlocks " + unlockMap.Name;
                }
            }
            return "Great treasures are out there waiting to be found...";

        }
    }
}



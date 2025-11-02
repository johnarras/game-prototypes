using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Tavern.Services
{
    public interface ITavernService : IInjectable
    {
        string GetRumor(PartyData party, CrawlerWorld world);
    }

    public class TavernService : ITavernService
    {
        private IClientRandom _rand = null;
        private ICrawlerMapService _mapService = null;

        public string GetRumor(PartyData party, CrawlerWorld world)
        {
            if (world.QuestItems.Count < 1)
            {
                return "Lots of scary monsters out there.";
            }

            bool forceQuestItem = false;
            WorldQuestItem questItem = world.QuestItems[_rand.Next(world.QuestItems.Count)];

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
                    CrawlerMap finalMap = dungeonExits[_rand.Next() % dungeonExits.Count];

                    questItem = world.QuestItems.FirstOrDefault(x => x.IdKey == finalMap.IdKey);

                    if (_rand.NextDouble() < 0.80f)
                    {
                        forceQuestItem = true;
                    }
                }
            }

            if (!forceQuestItem && _rand.NextDouble() < 0.35f)
            {
                List<CrawlerMap> subMaps = world.Maps.Where(x => x.CrawlerMapTypeId == CrawlerMapTypes.Dungeon).ToList();

                CrawlerMap targetMap = subMaps[_rand.Next() % subMaps.Count];

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
                    CrawlerMap exitMap = exitMaps[_rand.Next(exitMaps.Count)];

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

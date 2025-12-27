using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class MapInfoHelper : IInfoHelper
    {

        protected ICrawlerService _crawlerService = null;
        protected ICrawlerWorldService _worldService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected ICrawlerMapService _mapService = null;
        protected IInfoService _infoService = null;

        public virtual long HelperKey => EntityTypes.Map;

        public List<IIdName> GetInfoChildren()
        {
            PartyData party = _crawlerService.GetParty();

            CrawlerWorld world = _worldService.GetWorld(party.WorldId).Result;
            return world.Maps.Cast<IIdName>().ToList();
        }

        public string GetTypeName()
        {
            return typeof(CrawlerMap).Name;
        }

        public bool OverviewTypeNameIsPlural()
        {
            return true;
        }

        public virtual List<string> GetInfoLines(long entityId)
        {

            List<String> lines = new List<string>();
            StatSettings statSettings = _gameData.Get<StatSettings>(_gs.ch);

            PartyData party = _crawlerService.GetParty();

            CrawlerWorld world = _worldService.GetWorld(party.WorldId).Result;

            CrawlerMap map = world.GetMap(entityId);

            if (map == null)
            {
                lines.Add("Missing Map");
                return lines;
            }

            EntranceMapData entranceData = _mapService.GetEntranceMap(party, world, map.IdKey);

            lines.Add("Map");
            lines.Add(map.Name);

            CrawlerMapType mapType = _gameData.Get<CrawlerMapSettings>(_gs.ch).Get(map.CrawlerMapTypeId);

            lines.Add("MapType: " + mapType.Name);

            if (entranceData.IsValid())
            {
                lines.Add("Found within " + _infoService.CreateInfoLink(entranceData.EntranceMap, entranceData.EntranceMapName)
                    + $" at ({entranceData.EnterX},{entranceData.EnterZ})");
            }

            UnitTypeSettings unitSettings = _gameData.Get<UnitTypeSettings>(_gs.ch);


            lines.Add("Monsters:");

            foreach (ZoneUnitSpawn spawn in map.ZoneUnits)
            {
                UnitType utype = unitSettings.Get(spawn.UnitTypeId);

                if (utype != null)
                {
                    lines.Add(_infoService.CreateInfoLink(utype));
                }
            }

            List<MapCellDetail> details = map.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

            if (entranceData.IsValid())
            {
                details = details.Where(x => x.EntityId != entranceData.EntranceMap.IdKey).ToList();
            }


            List<string> childLinks = new List<string>();

            foreach (MapCellDetail detail in details)
            {
                CrawlerMap otherMap = world.GetMap(detail.EntityId);

                if (otherMap != null && otherMap.BaseCrawlerMapId == otherMap.IdKey)
                {
                    childLinks.Add(_infoService.CreateInfoLink(otherMap) + $" at ({detail.X},{detail.Z})");
                }
            }

            if (childLinks.Count > 0)
            {
                lines.Add("Contains:");
                lines.AddRange(childLinks);
            }

            return lines;

        }
    }
}



using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Settings;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Zones.Settings;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Maps.Entities
{
    public class CrawlerMapGenData
    {
        public CrawlerWorld World { get; set; }
        public long MapTypeId { get; set; }
        public long Level { get; set; }
        public long FromMapId { get; set; }
        public int FromMapX { get; set; }
        public int FromMapZ { get; set; }
        public int CurrFloor { get; set; }
        public int MaxFloor { get; set; } 
        public string Name { get; set; }
        public long DungeonTypeId { get; set; }
        public CrawlerMap PrevMap { get; set; }
        public long ArtSeed { get; set; }
        public CrawlerMapType MapType { get; set; }
        public CrawlerMapGenType GenType { get; set; }
        public ZoneType ZoneType { get; set; }
        public long BuildingArtId { get; set; }
        public long BaseCrawlerMapId { get; set; }
        public long ForcedIdKey { get; set; }
        public List<ZoneUnitSpawn> SharedUnits { get; set; } = new List<ZoneUnitSpawn>();
        public List<CurrentUnitKeyword> UnitKeywords { get; set; } = new List<CurrentUnitKeyword>();
    }

}



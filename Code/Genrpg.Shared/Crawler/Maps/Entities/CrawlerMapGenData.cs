using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Zones.Settings;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    public class CrawlerMapGenData
    {
        public CrawlerWorld World { get; set; }
        public long MapTypeId { get; set; }
        public long Level { get; set; }
        public long LevelDelta { get; set; }
        public bool Looping { get; set; }
        public long FromMapId { get; set; }
        public int FromMapX { get; set; }
        public int FromMapZ { get; set; }
        public long CurrFloor { get; set; } = 1;
        public long MaxFloor { get; set; } = 1;
        public string Name { get; set; }
        public bool RandomWallsDungeon { get; set; }
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



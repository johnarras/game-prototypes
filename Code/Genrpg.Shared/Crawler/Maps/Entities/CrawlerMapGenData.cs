using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Zones.Settings;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    [MessagePackObject]
    public class CrawlerMapGenData
    {
        [Key(0)] public CrawlerWorld World { get; set; }
        [Key(1)] public long MapTypeId { get; set; }
        [Key(2)] public int Level { get; set; } = 0;
        [Key(3)] public bool Looping { get; set; }
        [Key(4)] public long FromMapId { get; set; }
        [Key(5)] public int FromMapX { get; set; }
        [Key(6)] public int FromMapZ { get; set; }
        [Key(7)] public int CurrFloor { get; set; } = 1;
        [Key(8)] public int MaxFloor { get; set; } = 1;
        [Key(9)] public string Name { get; set; }
        [Key(10)] public bool RandomWallsDungeon { get; set; }
        [Key(11)] public CrawlerMap PrevMap { get; set; }
        [Key(12)] public long ArtSeed { get; set; }
        [Key(13)] public CrawlerMapType MapType { get; set; }
        [Key(14)] public CrawlerMapGenType GenType { get; set; }
        [Key(15)] public ZoneType ZoneType { get; set; }
        [Key(16)] public long BuildingArtId { get; set; }
        [Key(17)] public long BaseCrawlerMapId { get; set; }
        [Key(18)] public List<ZoneUnitSpawn> SharedUnits { get; set; } = new List<ZoneUnitSpawn>();
        [Key(19)] public List<CurrentUnitKeyword> UnitKeywords { get; set; } = new List<CurrentUnitKeyword>();
    }

}

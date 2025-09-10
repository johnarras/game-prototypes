using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Travel.Settings
{
    [MessagePackObject]
    public class TravelSettings : ParentSettings<TravelPath>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class RoadSegment
    {
        [Key(0)] public long RoadId { get; set; }
        [Key(1)] public int Index { get; set; }
    }

    [MessagePackObject]
    public class TravelPath : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public long StartCityId { get; set; }
        [Key(9)] public long EndCityId { get; set; }
        [Key(10)] public double Difficulty { get; set; }
        [Key(11)] public double Length { get; set; }
        [Key(12)] public List<RoadSegment> Segments { get; set; } = new List<RoadSegment>();
    }

    public class TravelSettingsDto : ParentSettingsDto<TravelSettings, TravelPath> { }

    public class TravelSettingsLoader : ParentSettingsLoader<TravelSettings, TravelPath> { }

    public class TravelSettingsMapper : ParentSettingsMapper<TravelSettings, TravelPath, TravelSettingsDto> { }

}

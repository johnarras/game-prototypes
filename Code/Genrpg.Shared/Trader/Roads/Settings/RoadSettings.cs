using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Trader.Roads.Settings
{
    [MessagePackObject]
    public class RoadSettings : ParentSettings<Road>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class Road : ChildSettings, IIndexedGameItem
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
        [Key(10)] public double Maintenance { get; set; }
        [Key(11)] public double Difficulty { get; set; }
        [Key(12)] public double Danger { get; set; }
        [Key(13)] public double SummerRain { get; set; }
        [Key(14)] public double SummerHeat { get; set; }
        [Key(15)] public double WinterRain { get; set; }
        [Key(16)] public double WinterHeat { get; set; }
        [Key(17)] public double Length { get; set; }
    }

    public class RoadSettingsDto : ParentSettingsDto<RoadSettings, Road> { }

    public class RoadSettingsLoader : ParentSettingsLoader<RoadSettings, Road> { }

    public class RoadSettingsMapper : ParentSettingsMapper<RoadSettings, Road, RoadSettingsDto> { }

    public class RoadEntityHelper : BaseEntityHelper<RoadSettings, Road>
    {
        public override long Key => EntityTypes.Road;
    }
}

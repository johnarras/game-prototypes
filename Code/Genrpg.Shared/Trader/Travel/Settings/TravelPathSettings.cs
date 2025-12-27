using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Travel.Settings
{
    public class TravelPathSettings : ParentSettings<TravelPath>
    {
        public override string Id { get; set; }
    }

    public class RoadSegment
    {
        public long RoadId { get; set; }
        public int Index { get; set; }
    }

    public class TravelPath : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public long StartCityId { get; set; }
        public long EndCityId { get; set; }
        public double Difficulty { get; set; }
        public double Length { get; set; }
        public List<RoadSegment> Segments { get; set; } = new List<RoadSegment>();
    }

    public class TravelPathSettingsDto : ParentSettingsDto<TravelPathSettings, TravelPath>
    {
        public override List<TravelPath> Children { get; set; }
        public override TravelPathSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TravelPathSettingsLoader : ParentSettingsLoader<TravelPathSettings, TravelPath> { }

    public class TravelPathSettingsMapper : ParentSettingsMapper<TravelPathSettings, TravelPath, TravelPathSettingsDto> { }

}



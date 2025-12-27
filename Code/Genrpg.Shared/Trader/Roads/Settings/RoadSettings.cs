using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Roads.Settings
{
    public class RoadSettings : ParentSettings<Road>
    {
        public override string Id { get; set; }
    }

    public class Road : ChildSettings, IIndexedGameItem
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
        public double Maintenance { get; set; }
        public double Difficulty { get; set; }
        public double Danger { get; set; }
        public double SummerRain { get; set; }
        public double SummerHeat { get; set; }
        public double WinterRain { get; set; }
        public double WinterHeat { get; set; }
        public long Distance { get; set; }

        public long GetCityIdOnOtherEnd(long cityId)
        {
            return cityId == StartCityId ? EndCityId : StartCityId;
        }
    }

    public class RoadSettingsDto : ParentSettingsDto<RoadSettings, Road>
    {
        public override List<Road> Children { get; set; }
        public override RoadSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class RoadSettingsLoader : ParentSettingsLoader<RoadSettings, Road> { }

    public class RoadSettingsMapper : ParentSettingsMapper<RoadSettings, Road, RoadSettingsDto> { }

    public class RoadEntityHelper : BaseEntityHelper<RoadSettings, Road>
    {
        public override long HelperKey => EntityTypes.Road;
    }
}



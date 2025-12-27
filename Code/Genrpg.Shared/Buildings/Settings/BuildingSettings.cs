using MessagePack;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Buildings.Settings
{
    public class BuildingSettings : ParentConstantListSettings<BuildingType, BuildingTypes>
    {
        public override string Id { get; set; }
    }

    public class BuildingType : ChildSettings, IVariationIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public int Radius { get; set; } = 5;

        public bool IsCrawlerBuilding { get; set; }
        public int VariationCount { get; set; } = 1;
        public bool ShowNameplate { get; set; }
    }

    public class BuildingSettingsDto : ParentSettingsDto<BuildingSettings, BuildingType>
    {
        public override List<BuildingType> Children { get; set; }
        public override BuildingSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class BuildingSettingsLoader : ParentSettingsLoader<BuildingSettings, BuildingType> { }

    public class BuildingSettingsMapper : ParentSettingsMapper<BuildingSettings, BuildingType, BuildingSettingsDto> { }


    public class BuildingHelper : BaseEntityHelper<BuildingSettings, BuildingType>
    {
        public override long HelperKey => EntityTypes.Building;
    }

}



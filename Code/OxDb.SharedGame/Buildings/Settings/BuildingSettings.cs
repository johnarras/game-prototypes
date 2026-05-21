using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Buildings.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Buildings.Settings
{
    public class BuildingSettings : ParentConstantListSettings<BuildingType, BuildingTypes>
    {
        public override string Id { get; set; }
    }

    public class BuildingType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public bool IsCrawlerBuilding { get; set; }
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



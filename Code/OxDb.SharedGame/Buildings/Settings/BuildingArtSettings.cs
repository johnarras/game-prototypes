using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using System.Collections.Generic;

namespace OxDb.SharedGame.Buildings.Settings
{
    public class BuildingArtSettings : ParentSettings<BuildingArt>
    {
        public override string Id { get; set; }
    }
    public class BuildingArt : ChildSettings, IIndexedGameItem, IWeightedItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public double Weight { get; set; }

    }

    public class BuildingArtSettingsDto : ParentSettingsDto<BuildingArtSettings, BuildingArt>
    {
        public override List<BuildingArt> Children { get; set; }
        public override BuildingArtSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class BuildingArtSettingsLoader : ParentSettingsLoader<BuildingArtSettings, BuildingArt> { }

    public class BuildingArtSettingsMapper : ParentSettingsMapper<BuildingArtSettings, BuildingArt, BuildingArtSettingsDto> { }


}



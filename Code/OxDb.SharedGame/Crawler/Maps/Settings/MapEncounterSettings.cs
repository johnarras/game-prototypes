using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Maps.Settings
{
    public class MapEncounterSettings : ParentSettings<MapEncounterType>
    {
        public override string Id { get; set; }
        public double EncounterChance { get; set; }
        public double CornerSmallRoomEncounterChance { get; set; }
    }

    public class MapEncounterType : ChildSettings, IIndexedGameItem, IWeightedItem
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
        public bool CanRepeat { get; set; }
        public bool CanBeCleansed { get; set; }
        public bool IsCornerSmallRoomItem { get; set; }

    }

    public class MapEncounterSettingsDto : ParentSettingsDto<MapEncounterSettings, MapEncounterType>
    {
        public override List<MapEncounterType> Children { get; set; }
        public override MapEncounterSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class MapEncounterSettingsLoader : ParentSettingsLoader<MapEncounterSettings, MapEncounterType> { }

    public class MapEncounterSettingsMapper : ParentSettingsMapper<MapEncounterSettings, MapEncounterType, MapEncounterSettingsDto> { }

    public class MapEncounterEntityHelper : BaseEntityHelper<MapEncounterSettings, MapEncounterType>
    {
        public override long HelperKey => EntityTypes.MapEncounter;
    }
}



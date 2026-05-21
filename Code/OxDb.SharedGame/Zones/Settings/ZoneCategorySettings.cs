using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Zones.Settings
{
    public class ZoneCategorySettings : ParentSettings<ZoneCategory>
    {
        public override string Id { get; set; }
    }
    public class ZoneCategory : ChildSettings, IIndexedGameItem
    {
        public const int None = 0;
        public const int Doubloons = 1;


        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string PluralName { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

    }
    public class ZoneCategorySettingsDto : ParentSettingsDto<ZoneCategorySettings, ZoneCategory>
    {
        public override string Id { get; set; }
        public override List<ZoneCategory> Children { get; set; } = new List<ZoneCategory>();
        public override ZoneCategorySettings Parent { get; set; }
    }
    public class UnitCoinSettingsLoader : ParentSettingsLoader<ZoneCategorySettings, ZoneCategory> { }

    public class ZoneCategorySettingsMapper : ParentSettingsMapper<ZoneCategorySettings, ZoneCategory, ZoneCategorySettingsDto> { }


    public class ZoneCategoryHelper : BaseEntityHelper<ZoneCategorySettings, ZoneCategory>
    {
        public override long HelperKey => EntityTypes.ZoneCategory;
    }
}



using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Purchasing.Settings
{
    public class StoreThemeSettings : ParentSettings<StoreTheme>
    {
        public override string Id { get; set; }
    }

    public class StoreThemeSettingsDto : ParentSettingsDto<StoreThemeSettings, StoreTheme>
    {
        public override List<StoreTheme> Children { get; set; }
        public override StoreThemeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class StoreThemeSettingsLoader : ParentSettingsLoader<StoreThemeSettings, StoreTheme> { }

    public class StoreThemeSettingsMapper : ParentSettingsMapper<StoreThemeSettings, StoreTheme, StoreThemeSettingsDto> { }


    public class StoreTheme : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string Art { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
    }
}



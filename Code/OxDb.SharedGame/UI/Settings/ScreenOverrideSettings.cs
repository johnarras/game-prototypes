using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.UI.Settings
{
    public class ScreenOverride : ChildSettings, IId
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public long DefaultScreenNameId { get; set; }
        public long ReplaceScreenNameId { get; set; }

    }
    public class ScreenOverrideSettings : ParentSettings<ScreenOverride>
    {
        public override string Id { get; set; }
    }

    public class ScreenOverrideSettingsDto : ParentSettingsDto<ScreenOverrideSettings, ScreenOverride>
    {
        public override List<ScreenOverride> Children { get; set; }
        public override ScreenOverrideSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class ScreenOverrideSettingsLoader : ParentSettingsLoader<ScreenOverrideSettings, ScreenOverride> { }

    public class ScreenOverrideSettingsMapper : ParentSettingsMapper<ScreenOverrideSettings, ScreenOverride, ScreenOverrideSettingsDto> { }
}



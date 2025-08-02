using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Purchasing.Settings;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UI.Settings
{
    [MessagePackObject]
    public class ScreenOverride : ChildSettings, IId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public long DefaultScreenNameId { get; set; }
        [Key(5)] public long ReplaceScreenNameId { get; set; }

    }
    [MessagePackObject]
    public class ScreenOverrideSettings : ParentSettings<ScreenOverride>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class ScreenOverrideSettingsDto : ParentSettingsDto<ScreenOverrideSettings, ScreenOverride> { }
    public class ScreenOverrideSettingsLoader : ParentSettingsLoader<ScreenOverrideSettings, ScreenOverride> { }

    public class ScreenOverrideSettingsMapper : ParentSettingsMapper<ScreenOverrideSettings, ScreenOverride, ScreenOverrideSettingsDto> { }
}

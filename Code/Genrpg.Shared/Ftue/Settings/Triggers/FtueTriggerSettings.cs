using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Ftue.Settings.Triggers
{

    [MessagePackObject]
    public class FtueTrigger : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
    }

    [MessagePackObject]
    public class FtueTriggerSettings : ParentSettings<FtueTrigger>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class FtueTriggerSettingsDto : ParentSettingsDto<FtueTriggerSettings, FtueTrigger> { }
    public class FtueTriggerSettingsLoader : ParentSettingsLoader<FtueTriggerSettings, FtueTrigger> { }

    public class FtueTriggerSettingsMapper : ParentSettingsMapper<FtueTriggerSettings, FtueTrigger, FtueTriggerSettingsDto> { }


}

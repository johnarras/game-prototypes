using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;
using System;

namespace Genrpg.Shared.Time.GameData
{

    [MessagePackObject]
    public class TimeSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public bool UseOverrideTime { get; set; }
        [Key(2)] public DateTime OverrideTime { get; set; }


    }


    public class TimeSettingsLoader : NoChildSettingsLoader<TimeSettings> { }


    public class TimeSettingsDto : NoChildSettingsDto<TimeSettings> { }

    public class TimeSettingsMapper : NoChildSettingsMapper<TimeSettings, TimeSettingsDto> { }
}

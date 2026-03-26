using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using System;

namespace Genrpg.Shared.Time.Settings
{

    public class TimeSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public bool UseOverrideTime { get; set; }
        public DateTime OverrideTime { get; set; }


    }


    public class TimeSettingsLoader : NoChildSettingsLoader<TimeSettings> { }


    public class TimeSettingsDto : NoChildSettingsDto<TimeSettings>
    {
        public override TimeSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TimeSettingsMapper : NoChildSettingsMapper<TimeSettings, TimeSettingsDto> { }
}



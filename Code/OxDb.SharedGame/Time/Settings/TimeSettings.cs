using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using System;

namespace OxDb.SharedGame.Time.Settings
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



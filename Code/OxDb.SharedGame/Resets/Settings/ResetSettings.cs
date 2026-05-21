using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.Resets.Settings
{
    public class ResetSettings : NoChildSettings
    {
        public override string Id { get; set; }

        public int ResetHour { get; set; }
    }
    public class ResetSettingsLoader : NoChildSettingsLoader<ResetSettings> { }

    public class ResetSettingsDto : NoChildSettingsDto<ResetSettings>
    {
        public override ResetSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class ResetSettingsMapper : NoChildSettingsMapper<ResetSettings, ResetSettingsDto> { }
}



using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Resets.Settings
{
    public class ResetSettings : NoChildSettings
    {
        public override string Id { get; set; }

        public double ResetHour { get; set; }
    }
    public class ResetSettingsLoader : NoChildSettingsLoader<ResetSettings> { }

    public class ResetSettingsDto : NoChildSettingsDto<ResetSettings>
    {
        public override ResetSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class ResetSettingsMapper : NoChildSettingsMapper<ResetSettings, ResetSettingsDto> { }
}



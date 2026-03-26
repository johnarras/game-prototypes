using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Core.Settings
{
    public class CoreSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public string GameName { get; set; }
        public string GameVersion { get; set; }
        public string UnityProjectId { get; set; }
        public string BundleId { get; set; }
    }


    public class CoreSettingsLoader : NoChildSettingsLoader<CoreSettings> { }

    public class CoreSettingsDto : NoChildSettingsDto<CoreSettings>
    {
        public override CoreSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CoreSettingsMapper : NoChildSettingsMapper<CoreSettings, CoreSettingsDto> { }
}



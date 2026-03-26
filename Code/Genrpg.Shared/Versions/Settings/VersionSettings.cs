using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Versions.Settings
{
    public class VersionSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public int ClientVersion { get; set; }
        public int ServerVersion { get; set; }
        public int UserVersion { get; set; }
        public int CharacterVersion { get; set; }
    }

    public class VersionSettingsLoader : NoChildSettingsLoader<VersionSettings> { }

    public class VersionSettingsDto : NoChildSettingsDto<VersionSettings>
    {
        public override string Id { get; set; }
        public override VersionSettings Parent { get; set; }
    }

    public class VersionSettingsMapper : NoChildSettingsMapper<VersionSettings, VersionSettingsDto> { }
}



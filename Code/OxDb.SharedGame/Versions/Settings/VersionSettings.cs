using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.Versions.Settings
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



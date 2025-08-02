
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Versions.Settings
{
    [MessagePackObject]
    public class VersionSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int ClientVersion { get; set; }
        [Key(2)] public int ServerVersion { get; set; }
        [Key(3)] public int UserVersion { get; set; }
        [Key(4)] public int CharacterVersion { get; set; }
    }

    public class VersionSettingsLoader : NoChildSettingsLoader<VersionSettings> { }

    public class VersionSettingsDto : NoChildSettingsDto<VersionSettings> { }

    public class VersionSettingsMapper : NoChildSettingsMapper<VersionSettings, VersionSettingsDto> { }
}

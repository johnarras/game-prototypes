using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Core.Settings
{
    [MessagePackObject]
    public class CoreSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string GameName { get; set; }
        [Key(2)] public string GameVersion { get; set; }
        [Key(3)] public string UnityProjectId { get; set; }
        [Key(4)] public string BundleId { get; set; }
    }


    public class CoreSettingsLoader : NoChildSettingsLoader<CoreSettings> { }

    public class CoreSettingsDto : NoChildSettingsDto<CoreSettings> { }

    public class CoreSettingsMapper : NoChildSettingsMapper<CoreSettings, CoreSettingsDto> { }
}

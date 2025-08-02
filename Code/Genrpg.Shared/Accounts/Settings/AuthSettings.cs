using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Accounts.Settings
{
    [MessagePackObject]
    public class AuthSettings : NoChildSettings
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string MinClientVersion { get; set; }
    }

    public class AuthSettingsLoader : NoChildSettingsLoader<AuthSettings> { }

}

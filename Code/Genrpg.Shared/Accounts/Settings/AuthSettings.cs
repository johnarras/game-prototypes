using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Accounts.Settings
{
    public class AuthSettings : NoChildSettings
    {
        public override string Id { get; set; }
        public string MinClientVersion { get; set; }
    }

    public class AuthSettingsLoader : NoChildSettingsLoader<AuthSettings> { }

}



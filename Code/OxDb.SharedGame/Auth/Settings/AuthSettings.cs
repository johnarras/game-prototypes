using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;

namespace OxDb.SharedGame.Auth.Settings
{
    public class AuthSettings : NoChildSettings
    {
        public override string Id { get; set; }
        public string MinClientVersion { get; set; }
    }

    public class AuthSettingsLoader : NoChildSettingsLoader<AuthSettings> { }

}



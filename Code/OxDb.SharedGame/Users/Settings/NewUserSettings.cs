using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.Users.Settings
{
    public class NewUserSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }

    }


    public class NewUserSettingsLoader : NoChildSettingsLoader<NewUserSettings> { }

    public class NewUserSettingsDto : NoChildSettingsDto<NewUserSettings>
    {
        public override NewUserSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class NewUserSettingsMapper : NoChildSettingsMapper<NewUserSettings, NewUserSettingsDto> { }
}



using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Users.Settings
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



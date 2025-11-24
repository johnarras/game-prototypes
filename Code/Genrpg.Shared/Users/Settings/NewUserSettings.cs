using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Users.Settings
{
    [MessagePackObject]
    public class NewUserSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }

    }


    public class NewUserSettingsLoader : NoChildSettingsLoader<NewUserSettings> { }

    public class NewUserSettingsDto : NoChildSettingsDto<NewUserSettings> { }

    public class NewUserSettingsMapper : NoChildSettingsMapper<NewUserSettings, NewUserSettingsDto> { }
}

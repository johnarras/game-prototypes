using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.BoardGame.Settings;

namespace Genrpg.Shared.Users.Settings
{
    [MessagePackObject]
    public class NewUserSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public long Tokens { get; set; }
        [Key(2)] public long Energy { get; set; } 
        [Key(3)] public long Money { get; set; }
        [Key(4)] public long EnergyPerHour { get; set; }
        [Key(5)] public long TotalEnergyStorage { get; set; }
        [Key(6)] public long MarkerId { get; set; }
        [Key(7)] public long MarkerTier { get; set; }

    }


    public class NewUserSettingsLoader : NoChildSettingsLoader<NewUserSettings> { }

    public class NewUserSettingsDto : NoChildSettingsDto<NewUserSettings> { }

    public class NewUserSettingsMapper : NoChildSettingsMapper<NewUserSettings, NewUserSettingsDto> { }
}

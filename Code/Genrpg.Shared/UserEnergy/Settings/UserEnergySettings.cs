using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using System;

namespace Genrpg.Shared.UserEnergy.Settings
{
    public class UserEnergySettings : NoChildSettings
    {
        public override string Id { get; set; }

        public double HourlyRegenPercent { get; set; } = 0.25;

        public int StartStorage { get; set; } = 40;

        public int LevelsPerIncrement { get; set; } = 5;

        public int IncrementQuantity { get; set; } = 5;

        public int StorageCap { get; set; } = 80;

        public int GetMaxStorage(long level)
        {
            return (int)Math.Min(StorageCap, StartStorage + level / LevelsPerIncrement * IncrementQuantity);
        }

        public double EnergyPerHour(long level)
        {
            return GetMaxStorage(level) * HourlyRegenPercent;
        }

    }
    public class UserEnergySettingsLoader : NoChildSettingsLoader<UserEnergySettings> { }

    public class UserEnergySettingsDto : NoChildSettingsDto<UserEnergySettings>
    {
        public override string Id { get; set; }
        public override UserEnergySettings Parent { get; set; }
    }

    public class UserEnergySettingsMapper : NoChildSettingsMapper<UserEnergySettings, UserEnergySettingsDto> { }
}



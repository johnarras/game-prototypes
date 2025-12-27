using MessagePack;

using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.TimeOfDay.Settings
{
    public class StatRegenHours
    {
        public long StatTypeId { get; set; }
        public double RegenHours { get; set; }
        public string Name { get; set; } = null;
    }

    public class TimeOfDaySettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public double DailyResetHour { get; set; }
        public double BaseMoveMinutes { get; set; }
        public double CombatRoundMinutes { get; set; }
        public double RestHours { get; set; }


        public double EatHours { get; set; }
        public double DrinkHours { get; set; }
        public double RumorHours { get; set; }


        public List<StatRegenHours> RegenHours { get; set; }

        public double LevitateSpeedup { get; set; }

        public double MoveSpeedIncreasePerExtraInventoryItem { get; set; }
    }


    public class TimeOfDaySettingsLoader : NoChildSettingsLoader<TimeOfDaySettings> { }


    public class TimeOfDaySettingsDto : NoChildSettingsDto<TimeOfDaySettings>
    {
        public override TimeOfDaySettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TimeOfDaySettingsMapper : NoChildSettingsMapper<TimeOfDaySettings, TimeOfDaySettingsDto> { }
}



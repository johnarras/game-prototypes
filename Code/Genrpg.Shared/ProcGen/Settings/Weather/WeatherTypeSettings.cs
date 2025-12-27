using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using System.Collections.Generic;

namespace Genrpg.Shared.ProcGen.Settings.Weather
{
    public class WeatherTypeSettings : ParentSettings<WeatherType>
    {
        public override string Id { get; set; }
    }

    public class WeatherTypeSettingsDto : ParentSettingsDto<WeatherTypeSettings, WeatherType>
    {
        public override string Id { get; set; }
        public override List<WeatherType> Children { get; set; } = new List<WeatherType>();
        public override WeatherTypeSettings Parent { get; set; }
    }
    public class WeatherTypeSettingsLoader : ParentSettingsLoader<WeatherTypeSettings, WeatherType> { }

    public class WeatherSettingsMapper : ParentSettingsMapper<WeatherTypeSettings, WeatherType, WeatherTypeSettingsDto> { }


}



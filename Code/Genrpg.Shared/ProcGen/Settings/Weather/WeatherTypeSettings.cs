using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Settings.Weather
{
    [MessagePackObject]
    public class WeatherTypeSettings : ParentSettings<WeatherType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class WeatherTypeSettingsDto : ParentSettingsDto<WeatherTypeSettings, WeatherType> { }
    public class WeatherTypeSettingsLoader : ParentSettingsLoader<WeatherTypeSettings, WeatherType> { }

    public class WeatherSettingsMapper : ParentSettingsMapper<WeatherTypeSettings, WeatherType, WeatherTypeSettingsDto> { }


}

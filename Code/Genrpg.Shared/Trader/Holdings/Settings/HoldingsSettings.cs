using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Trader.Holdings.Settings
{
    public class HoldingsSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
    }


    public class HoldingsSettingsLoader : NoChildSettingsLoader<HoldingsSettings> { }


    public class HoldingsSettingsDto : NoChildSettingsDto<HoldingsSettings>
    {
        public override HoldingsSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class HoldingsSettingsMapper : NoChildSettingsMapper<HoldingsSettings, HoldingsSettingsDto> { }
}



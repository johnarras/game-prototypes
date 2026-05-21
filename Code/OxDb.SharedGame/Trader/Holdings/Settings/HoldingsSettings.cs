using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.Trader.Holdings.Settings
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



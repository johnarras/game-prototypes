using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Trader.TradeEconomy.Settings
{
    public class TradeEconomySettings : NoChildSettings
    {
        public override string Id { get; set; }

        public double SellPricePercent { get; set; }

        public long MaxCostDistance { get; set; } // 10000?
        public double MinConsumerPriceScale { get; set; } // 3?
        public double MaxConsumerPriceScale { get; set; }  // 8?

        public double SmallProducerPriceScale { get; set; } // 2
        public double BigProducerPriceScale { get; set; } // 1



    }

    public class TradeEconomySettingsLoader : NoChildSettingsLoader<TradeEconomySettings> { }


    public class TradeEconomySettingsDto : NoChildSettingsDto<TradeEconomySettings>
    {
        public override TradeEconomySettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TradeEconomySettingsMapper : NoChildSettingsMapper<TradeEconomySettings, TradeEconomySettingsDto> { }

}



using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;

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

}



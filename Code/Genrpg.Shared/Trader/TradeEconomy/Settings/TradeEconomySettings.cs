using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;

namespace Genrpg.Shared.Trader.TradeEconomy.Settings
{
    [MessagePackObject]
    public class TradeEconomySettings : NoChildSettings
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public double SellPricePercent { get; set; }

        [Key(2)] public long MaxCostDistance { get; set; } // 10000?
        [Key(3)] public double MinConsumerPriceScale { get; set; } // 3?
        [Key(4)] public double MaxConsumerPriceScale { get; set; }  // 8?

        [Key(5)] public double SmallProducerPriceScale { get; set; } // 2
        [Key(6)] public double BigProducerPriceScale { get; set; } // 1



    }

    public class TradeEconomySettingsLoader : NoChildSettingsLoader<TradeEconomySettings> { }

}

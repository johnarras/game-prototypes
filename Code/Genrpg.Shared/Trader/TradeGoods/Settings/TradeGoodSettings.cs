using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Trader.TradeGoods.Settings
{
    [MessagePackObject]
    public class TradeGoodSettings : ParentSettings<TradeGood>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class TradeGood : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public string CategoryName { get; set; }
        [Key(9)] public double Density { get; set; }
        [Key(10)] public long UnitWeight { get; set; }
        [Key(11)] public long PricePerUnit { get; set; }
        [Key(12)] public long PricePerWeight { get; set; }
        [Key(13)] public long YearlyProduction { get; set; }
    }

    public class TradeGoodSettingsDto : ParentSettingsDto<TradeGoodSettings, TradeGood> { }

    public class TradeGoodSettingsLoader : ParentSettingsLoader<TradeGoodSettings, TradeGood> { }

    public class TradeGoodSettingsMapper : ParentSettingsMapper<TradeGoodSettings, TradeGood, TradeGoodSettingsDto> { }


    public class TradeGoodEntityHelper : BaseEntityHelper<TradeGoodSettings, TradeGood>
    {
        public override long Key => EntityTypes.TradeGood;
    }
}

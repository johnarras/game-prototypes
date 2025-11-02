using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.TradeGoods.Settings
{
    [MessagePackObject]
    public class TradeGoodSettings : ParentSettings<TradeGood>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class TradeGoodProducerCity
    {
        [Key(0)] public long CityId { get; set; }
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
        [Key(9)] public double Price { get; set; }
        [Key(10)] public SmallIdLongCollection CityBuyCosts { get; set; } = new SmallIdLongCollection();
        [Key(11)] public List<TradeGoodProducerCity> ProducerCities { get; set; } = new List<TradeGoodProducerCity>();
    }

    public class TradeGoodSettingsDto : ParentSettingsDto<TradeGoodSettings, TradeGood> { }

    public class TradeGoodSettingsLoader : ParentSettingsLoader<TradeGoodSettings, TradeGood> { }

    public class TradeGoodSettingsMapper : ParentSettingsMapper<TradeGoodSettings, TradeGood, TradeGoodSettingsDto> { }


    public class TradeGoodEntityHelper : BaseEntityHelper<TradeGoodSettings, TradeGood>
    {
        public override long Key => EntityTypes.TradeGood;
    }
}

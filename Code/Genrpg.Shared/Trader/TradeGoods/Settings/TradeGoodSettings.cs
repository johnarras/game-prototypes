using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.TradeGoods.Settings
{
    public class TradeGoodSettings : ParentSettings<TradeGood>
    {
        public override string Id { get; set; }
    }

    public class TradeGoodProducerCity
    {
        public long CityId { get; set; }
    }

    public class TradeGood : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public string CategoryName { get; set; }
        public double Price { get; set; }
        public SmallIdLongCollection CityBuyCosts { get; set; } = new SmallIdLongCollection();
        public List<TradeGoodProducerCity> ProducerCities { get; set; } = new List<TradeGoodProducerCity>();
    }

    public class TradeGoodSettingsDto : ParentSettingsDto<TradeGoodSettings, TradeGood>
    {
        public override List<TradeGood> Children { get; set; }
        public override TradeGoodSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TradeGoodSettingsLoader : ParentSettingsLoader<TradeGoodSettings, TradeGood> { }

    public class TradeGoodSettingsMapper : ParentSettingsMapper<TradeGoodSettings, TradeGood, TradeGoodSettingsDto> { }

    public class TradeGoodEntityHelper : BaseEntityHelper<TradeGoodSettings, TradeGood>
    {
        public override long HelperKey => EntityTypes.TradeGood;
    }
}



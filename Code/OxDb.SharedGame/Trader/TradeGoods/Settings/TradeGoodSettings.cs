using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.TradeGoods.Settings
{
    public class TradeGoodSettings : ParentSettings<TradeGood>
    {
        public override string Id { get; set; }
    }

    public class TradeGoodProducerCity
    {
        public long CityId { get; set; }
        public string Name { get; set; }
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
        public long Price { get; set; }

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



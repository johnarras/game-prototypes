using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Cities.Settings
{
    public class CitySettings : ParentSettings<City>
    {
        public override string Id { get; set; }
    }

    public class CityTradeGood
    {
        public long TradeGoodId { get; set; }
        public double ProductionScale { get; set; }
        public double PriceScale { get; set; }
    }

    public class CityAnimal
    {
        public long AnimalTypeId { get; set; }
        public double PriceScale { get; set; }
    }

    public class City : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public string AncientName { get; set; }
        public long Population { get; set; }
        public int MapPixelX { get; set; }
        public int MapPixelY { get; set; }
        public long BiomeTypeId { get; set; }
        public long CultureTypeId { get; set; }
        public List<CityTradeGood> TradeGoodsProduced { get; set; } = new List<CityTradeGood>();
        public List<CityAnimal> Animals { get; set; } = new List<CityAnimal>();
        public SmallIdLongCollection TradeGoodBuyCosts { get; set; } = new SmallIdLongCollection();
    }

    public class CitySettingsDto : ParentSettingsDto<CitySettings, City>
    {
        public override List<City> Children { get; set; }
        public override CitySettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CitySettingsLoader : ParentSettingsLoader<CitySettings, City> { }

    public class CitySettingsMapper : ParentSettingsMapper<CitySettings, City, CitySettingsDto> { }

    public class CityEntityHelper : BaseEntityHelper<CitySettings, City>
    {
        public override long HelperKey => EntityTypes.City;
    }
}



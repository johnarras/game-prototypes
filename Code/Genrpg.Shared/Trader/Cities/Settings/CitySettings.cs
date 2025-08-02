using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Cities.Settings
{
    [MessagePackObject]
    public class CitySettings : ParentSettings<City>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class CityTradeGood
    {
        [Key(0)] public long TradeGoodId { get; set; }
        [Key(1)] public double ProductionScale { get; set; }
        [Key(2)] public double PriceScale { get; set; }
    }

    [MessagePackObject]
    public class CityAnimal
    {
        [Key(0)] public long AnimalId { get; set; }
        [Key(1)] public double PriceScale { get; set; }
    }


    [MessagePackObject]
    public class City : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public string AncientName { get; set; }
        [Key(9)] public long Population { get; set; }
        [Key(10)] public double Latitude { get; set; }
        [Key(11)] public double Longitude { get; set; }
        [Key(12)] public int MapPixelX { get; set; }
        [Key(13)] public int MapPixelY { get; set; }
        [Key(14)] public List<CityTradeGood> TradeGoods { get; set; } = new List<CityTradeGood>();
        [Key(15)] public List<CityAnimal> Animals { get; set; } = new List<CityAnimal>();
    }

    public class CitySettingsDto : ParentSettingsDto<CitySettings, City> { }

    public class CitySettingsLoader : ParentSettingsLoader<CitySettings, City> { }

    public class CitySettingsMapper : ParentSettingsMapper<CitySettings, City, CitySettingsDto> { }

    public class CityEntityHelper : BaseEntityHelper<CitySettings, City>
    {
        public override long Key => EntityTypes.City;
    }
}

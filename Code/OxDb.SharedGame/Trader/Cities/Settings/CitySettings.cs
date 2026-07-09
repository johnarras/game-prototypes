using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.Cities.Settings
{
    public class CitySettings : ParentSettings<City>
    {
        public override string Id { get; set; }
    }

    public class CityTradeGood
    {
        public string Name { get; set; }
        public long TradeGoodId { get; set; }
    }

    public class CityCaravanMember
    {
        public long CaravanMemberId { get; set; }
        public string Name { get; set; }
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
        public int MapPixelZ { get; set; }
        public long BiomeTypeId { get; set; }
        public long CultureTypeId { get; set; }

        public List<CityTradeGood> TradeGoodsProduced { get; set; } = new List<CityTradeGood>();
        public List<CityCaravanMember> CaravanMembersForSale { get; set; } = new List<CityCaravanMember>();
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



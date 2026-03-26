using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.CurrencySpend.Constants;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using Genrpg.Shared.Trader.TradeGoods.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.CurrencySpend.SpendHelpers
{

    public class TradeGoodsSpendLocationHelper : BaseSpendLocationHelper
    {
        public override long HelperKey => SpendLocations.TradeGoods;

        public override async Task<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity)
        {
            List<SpendType> validSpendTypes = new List<SpendType>();

            CoreData coreData = await lookup.GetAsync<CoreData>();

            SpendLocation loc = GetSpendLocation(coreData);

            FullSpendLocation fullSpendLoc = new FullSpendLocation()
            {
                Location = GetSpendLocation(coreData)
            };

            TradeGoodSettings tradeGoodSettings = _gameData.Get<TradeGoodSettings>(coreData);

            IReadOnlyList<TradeGood> allTradeGoods = tradeGoodSettings.GetData();

            CaravanPosition pos = _caravanService.GetPosition(coreData);

            City city = pos.GetCurrentCity();

            List<CityTradeGood> tradeGoods = new List<CityTradeGood>();

            if (useCurrentCity)
            {
                if (city == null)
                {
                    return fullSpendLoc;
                }
                else
                {
                    tradeGoods = city.TradeGoodsProduced.ToList();
                }
            }
            else
            {
                foreach (TradeGood tradeGood in allTradeGoods)
                {
                    tradeGoods.Add(new CityTradeGood() { TradeGoodId = tradeGood.IdKey });
                }

                tradeGoods = tradeGoods.OrderBy(x => Guid.NewGuid()).ToList();

                if (tradeGoods.Count > 10)
                {
                    tradeGoods = tradeGoods.Take(10).ToList();
                }
            }

            fullSpendLoc.IsValid = true;


            foreach (CityTradeGood cityTradeGood in tradeGoods)
            {
                TradeGood tradeGood = tradeGoodSettings.Get(cityTradeGood.TradeGoodId);

                if (tradeGood == null || tradeGood.Price < 1)
                {
                    continue;
                }

                SpendType stype = new SpendType()
                {
                    SpendCoreCurrencyTypeId = CoreCurrencyTypes.Coins,
                    SpendQuantity = tradeGood.Price,
                    Index = tradeGood.IdKey,
                    Name = tradeGood.Name,
                    Desc = tradeGood.Desc,
                    MaxTimes = 1,
                    MinLevel = 1,
                };

                stype.Rewards.Add(new SpendReward()
                {
                    EntityTypeId = EntityTypes.TradeGood,
                    EntityId = tradeGood.IdKey,
                    Quantity = 1,
                });

                fullSpendLoc.SpendTypes.Add(stype);
            }

            return fullSpendLoc;
        }
    }
}

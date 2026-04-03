using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Maps.Services;
using Genrpg.Shared.Trader.TradeEconomy.Settings;
using Genrpg.Shared.Trader.TradeGoods.Settings;
using Genrpg.Shared.Trader.TradeGoods.WebApi;
using Genrpg.Shared.Trader.Travel.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.TradeGoods.Services
{
    public interface ITradeGoodService : IInjectable
    {
        Task<AddTradeGoodToCaravanResponse> AddTradeGoodToCaravan(IUnitDataLookup lookup, long tradeGoodId, long forcedUniqueId = 0);
        Task<RemoveTradeGoodFromCaravanResponse> RemoveTradeGoodFromCaravan(IUnitDataLookup lookup, long tradeGoodId, long sellValue, long uniqueId);

        Task<long> GetSellValueAtPosition(IUnitDataLookup lookup, long tradeGoodId, long x, long y);
    }


    public class TradeGoodService : ITradeGoodService
    {
        private IGameData _gameData = null;
        private ICaravanService _caravanService = null;
        private ITraderMapService _mapService = null;
        private ICalcAttributeService _calcAttributeService = null;

        private IRewardService _rewardService = null;

        public async Task<AddTradeGoodToCaravanResponse> AddTradeGoodToCaravan(IUnitDataLookup lookup, long tradeGoodId, long forcedUniqueId = 0)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();
            AddTradeGoodToCaravanResponse result = new AddTradeGoodToCaravanResponse()
            {
                Success = false,
                Travel = _caravanService.GetTravelInfo(coreData),
            };


            CaravanData caravanData = await lookup.GetAsync<CaravanData>();



            long newUniqueId = forcedUniqueId;

            if (newUniqueId == 0)
            {
                newUniqueId = ++coreData.UniqueId;
            }

            if (coreData.UniqueId <= newUniqueId)
            {
                coreData.UniqueId = newUniqueId; 
            }

            caravanData.TradeGoods.Add(new CaravanTradeGood() { TradeGoodId = tradeGoodId, UniqueId = newUniqueId });

            await _calcAttributeService.CalcBuffs(lookup);

            result.TradeGoodId = tradeGoodId;
            result.Travel = _caravanService.GetTravelInfo(coreData);
            result.UniqueId = newUniqueId;
            result.Success = true;

            return result;
        }

        public async Task<RemoveTradeGoodFromCaravanResponse> RemoveTradeGoodFromCaravan(IUnitDataLookup lookup, long tradeGoodId, long sellValue, long uniqueId)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();
            RemoveTradeGoodFromCaravanResponse response = new RemoveTradeGoodFromCaravanResponse()
            {
                Success = false,
                Travel = _caravanService.GetTravelInfo(coreData),
                UniqueId = uniqueId,
                SellValue = sellValue,
            };


            CaravanPosition position = _caravanService.GetPosition(coreData);

            if (position.GetCurrentCity() == null)
            {
                response.ErrorMessage = "You can only sell trade goods in cities.";
                return response;
            }

            CaravanData caravanData = await lookup.GetAsync<CaravanData>();
            CaravanTradeGood tradeGood = caravanData.TradeGoods.FirstOrDefault(x => x.TradeGoodId == tradeGoodId && x.UniqueId == uniqueId);

            if (tradeGood == null)
            {
                response.ErrorMessage = "You don't have that item.";
                return response;
            }

            TradeEconomySettings econSettings = _gameData.Get<TradeEconomySettings>(coreData);

            TravelSettings travelSettings = _gameData.Get<TravelSettings>(coreData);

            long serverSellValue = await GetSellValueAtPosition(lookup, tradeGoodId, position.CurrX, position.CurrY);

            if (serverSellValue != sellValue)
            {
                response.ErrorMessage = "Mismatch between client and server sell values.";
                return response;
            }

            await _rewardService.GiveReward(lookup, EntityTypes.CoreCurrency, CoreCurrencyTypes.Coins, sellValue, null, response.UniqueId, null);

            if (tradeGood == null)
            {
                response.ErrorMessage = "You don't have that item.";
                return response;
            }

            caravanData.TradeGoods.Remove(tradeGood);

            await _calcAttributeService.CalcBuffs(lookup);

            response.TradeGoodId = tradeGoodId;
            response.Travel = _caravanService.GetTravelInfo(coreData);

            response.Success = true;

            return response;
        }

        public async Task<long> GetSellValueAtPosition(IUnitDataLookup lookup, long tradeGoodId, long x, long y)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();

            TradeGood tg = _gameData.Get<TradeGoodSettings>(coreData).Get(tradeGoodId);

            if (tg == null || tg.Price < 1)
            {
                return 0;
            }

            TradeEconomySettings econSettings = _gameData.Get<TradeEconomySettings>(coreData);

            if (econSettings == null || econSettings.MaxCostDistance < 100)
            {
                return 0;
            }

            CitySettings citySettings = _gameData.Get<CitySettings>(coreData);

            double closestDistance = -1;

            foreach (TradeGoodProducerCity tradeGoodProducerCity in tg.ProducerCities)
            {
                City city = citySettings.Get(tradeGoodProducerCity.CityId);

                if (city != null)
                {
                    long distance = await _mapService.GetDistanceBetweenPoints(lookup, x, y, city.MapPixelX, city.MapPixelY);

                    if (distance < closestDistance || closestDistance == -1)
                    {
                        closestDistance = distance;
                    }
                }
            }

            double startPrice = tg.Price;
            double sellPriceDistanceScale = 1.0f;
            if (closestDistance > 0)
            {
                double distancePct = Math.Min(1, 1.0 * closestDistance / econSettings.MaxCostDistance);

                sellPriceDistanceScale = econSettings.MinConsumerPriceScale * (1 - distancePct) + econSettings.MaxConsumerPriceScale * (distancePct);
            }

            long finalPrice = (long)(econSettings.SellPricePercent * startPrice * sellPriceDistanceScale);

            return finalPrice;
        }
    }
}



using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.PlayerData;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Cities.Settings;
using OxDb.SharedGame.Trader.Maps.Services;
using OxDb.SharedGame.Trader.TradeEconomy.Settings;
using OxDb.SharedGame.Trader.TradeGoods.Settings;
using OxDb.SharedGame.Trader.TradeGoods.WebApi;
using OxDb.SharedGame.Trader.Travel.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.TradeGoods.Services
{
    public interface ITradeGoodService : IInjectable
    {
        ValueTask<AddTradeGoodToCaravanResponse> AddTradeGoodToCaravan(IUnitDataLookup lookup, long tradeGoodId, long forcedUniqueId = 0);
        ValueTask<RemoveTradeGoodFromCaravanResponse> RemoveTradeGoodFromCaravan(IUnitDataLookup lookup, long tradeGoodId, long sellValue, long uniqueId);

        ValueTask<long> GetSellValueAtPosition(IUnitDataLookup lookup, long tradeGoodId, long x, long z);
    }


    public class TradeGoodService : ITradeGoodService
    {
        private IGameData _gameData = null;
        private ICaravanService _caravanService = null;
        private ITraderMapService _mapService = null;
        private ICalcAttributeService _calcAttributeService = null;

        private IRewardService _rewardService = null;

        public async ValueTask<AddTradeGoodToCaravanResponse> AddTradeGoodToCaravan(IUnitDataLookup lookup, long tradeGoodId, long forcedUniqueId = 0)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();
            AddTradeGoodToCaravanResponse result = new AddTradeGoodToCaravanResponse()
            {
                Success = false,
                Travel = await _caravanService.GetTravelInfo(lookup),
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
            result.Travel = await _caravanService.GetTravelInfo(lookup);
            result.UniqueId = newUniqueId;
            result.Success = true;

            return result;
        }

        public async ValueTask<RemoveTradeGoodFromCaravanResponse> RemoveTradeGoodFromCaravan(IUnitDataLookup lookup, long tradeGoodId, long sellValue, long uniqueId)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();
            RemoveTradeGoodFromCaravanResponse response = new RemoveTradeGoodFromCaravanResponse()
            {
                Success = false,
                Travel = await _caravanService.GetTravelInfo(lookup),
                UniqueId = uniqueId,
                SellValue = sellValue,
            };

            CaravanPosition position = await _caravanService.GetPosition(lookup);

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

            long serverSellValue = await GetSellValueAtPosition(lookup, tradeGoodId, position.CurrX, position.CurrZ);

            if (serverSellValue != sellValue)
            {
                response.ErrorMessage = "Mismatch between client and server sell values.";
                return response;
            }

            await _rewardService.GiveReward(lookup, EntityTypes.CoreCurrency, CoreCurrencyTypes.Coins, sellValue, RewardSources.SellTradeGood, null, response.UniqueId, null);

            if (tradeGood == null)
            {
                response.ErrorMessage = "You don't have that item.";
                return response;
            }

            caravanData.TradeGoods.Remove(tradeGood);

            await _calcAttributeService.CalcBuffs(lookup);

            response.TradeGoodId = tradeGoodId;
            response.Travel = await _caravanService.GetTravelInfo(lookup);

            response.Success = true;

            return response;
        }

        public async ValueTask<long> GetSellValueAtPosition(IUnitDataLookup lookup, long tradeGoodId, long x, long z)
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
                    long distance = await _mapService.GetDistanceBetweenPoints(lookup, x, z, city.MapPixelX, city.MapPixelZ);

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



using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.TradeEconomy.Settings;
using Genrpg.Shared.Trader.TradeGoods.WebApi;
using System.Linq;

namespace Genrpg.Shared.Trader.TradeGoods.Services
{
    public interface ITradeGoodService : IInjectable
    {
        AddTradeGoodToCaravanResult BuyTradeGood(CoreUserData userData, CaravanData caravanData, TraderStatData statData, long tradeGoodId);
        RemoveTradeGoodFromCaravanResult SellTradeGood(CoreUserData userData, CaravanData caravanData, TraderStatData statData, long tradeGoodId);
        AddTradeGoodToCaravanResult AddTradeGoodToCaravan(CoreUserData userData, CaravanData caravanData, TraderStatData statData, long tradeGoodId);
        RemoveTradeGoodFromCaravanResult RemoveTradeGoodFromCaravan(CoreUserData userData, CaravanData caravanData, TraderStatData statData, long tradeGoodId);
    }


    public class TradeGoodService : ITradeGoodService
    {
        private IGameData _gameData = null;
        private ICaravanService _caravanService = null;

        public AddTradeGoodToCaravanResult BuyTradeGood(CoreUserData userData, CaravanData caravanData, TraderStatData statData, long tradeGoodId)
        {
            AddTradeGoodToCaravanResult result = new AddTradeGoodToCaravanResult()
            {
                Success = false,
                Travel = _caravanService.GetTravelInfo(userData),
            };

            CaravanPosition position = userData.GetPosition();

            City city = _gameData.Get<CitySettings>(userData).Get(position.CityId);

            if (city == null)
            {
                result.ErrorMessage = "You can only buy trade goods in cities.";
                return result;
            }

            CityTradeGood tradeGood = city.TradeGoodsProduced.FirstOrDefault(x => x.TradeGoodId == tradeGoodId);

            if (tradeGood == null)
            {
                result.ErrorMessage = "This city doesn't produce that trade good.";
                return result;
            }

            long buyCost = city.TradeGoodBuyCosts.Get(tradeGoodId);


            if (buyCost == 0)
            {
                result.ErrorMessage = "This item cannot be bought in this city.";
                return result;
            }

            if (buyCost > userData.Currencies.Get(CoreCurrencyTypes.Coins))
            {
                result.ErrorMessage = "You don't have enough money to buy this.";
                return result;
            }

            result.BuyCost = buyCost;

            userData.Currencies.Add(CoreCurrencyTypes.Coins, -buyCost);

            result = AddTradeGoodToCaravan(userData, caravanData, statData, tradeGoodId);
            result.BuyCost = buyCost;
            return result;
        }

        public AddTradeGoodToCaravanResult AddTradeGoodToCaravan(CoreUserData userData, CaravanData caravanData, TraderStatData statData, long tradeGoodId)
        {

            AddTradeGoodToCaravanResult result = new AddTradeGoodToCaravanResult()
            {
                Success = false,
                Travel = _caravanService.GetTravelInfo(userData),
            };
            caravanData.TradeGoods.Add(new CaravanTradeGood() { TradeGoodId = tradeGoodId });

            _caravanService.UpdateCoreStatsFromCaravan(userData, caravanData, statData);

            result.TradeGoodId = tradeGoodId;
            result.Travel = _caravanService.GetTravelInfo(userData);

            result.Success = true;

            return result;
        }

        public RemoveTradeGoodFromCaravanResult SellTradeGood(CoreUserData userData, CaravanData caravanData, TraderStatData statData, long tradeGoodId)
        {
            RemoveTradeGoodFromCaravanResult result = new RemoveTradeGoodFromCaravanResult()
            {
                Success = false,
                Travel = _caravanService.GetTravelInfo(userData),
            };


            CaravanPosition position = userData.GetPosition();

            City city = _gameData.Get<CitySettings>(userData).Get(position.CityId);

            if (city == null)
            {
                result.ErrorMessage = "You can only sell trade goods in cities.";
                return result;
            }

            CaravanTradeGood tradeGood = caravanData.TradeGoods.FirstOrDefault(x => x.TradeGoodId == tradeGoodId);

            if (tradeGood == null)
            {
                result.ErrorMessage = "You don't have that item.";
                return result;
            }

            TradeEconomySettings econSettings = _gameData.Get<TradeEconomySettings>(userData);

            long sellValue = (long)(city.TradeGoodBuyCosts.Get(tradeGoodId) * econSettings.SellPricePercent);


            userData.Currencies.Add(CoreCurrencyTypes.Coins, sellValue);


            result = RemoveTradeGoodFromCaravan(userData, caravanData, statData, tradeGoodId);

            result.SellValue = sellValue;
            return result;

        }


        public RemoveTradeGoodFromCaravanResult RemoveTradeGoodFromCaravan(CoreUserData userData, CaravanData caravanData, TraderStatData statData, long tradeGoodId)
        {
            RemoveTradeGoodFromCaravanResult result = new RemoveTradeGoodFromCaravanResult()
            {
                Success = false,
                Travel = _caravanService.GetTravelInfo(userData),
            };

            CaravanTradeGood tradeGood = caravanData.TradeGoods.FirstOrDefault(x => x.TradeGoodId == tradeGoodId);

            if (tradeGood == null)
            {
                result.ErrorMessage = "You don't have that item.";
                return result;
            }


            caravanData.TradeGoods.Remove(tradeGood);

            _caravanService.UpdateCoreStatsFromCaravan(userData, caravanData, statData);

            result.TradeGoodId = tradeGoodId;
            result.Travel = _caravanService.GetTravelInfo(userData);

            result.Success = true;

            return result;
        }
    }
}



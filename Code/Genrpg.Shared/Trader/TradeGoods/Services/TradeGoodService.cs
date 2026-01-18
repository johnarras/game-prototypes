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
        AddTradeGoodToCaravanResult BuyTradeGood(CoreData coreData, CaravanData caravanData, TraderStatData statData, long tradeGoodId);
        RemoveTradeGoodFromCaravanResult SellTradeGood(CoreData coreData, CaravanData caravanData, TraderStatData statData, long tradeGoodId);
        AddTradeGoodToCaravanResult AddTradeGoodToCaravan(CoreData coreData, CaravanData caravanData, TraderStatData statData, long tradeGoodId);
        RemoveTradeGoodFromCaravanResult RemoveTradeGoodFromCaravan(CoreData coreData, CaravanData caravanData, TraderStatData statData, long tradeGoodId);
    }


    public class TradeGoodService : ITradeGoodService
    {
        private IGameData _gameData = null;
        private ICaravanService _caravanService = null;

        public AddTradeGoodToCaravanResult BuyTradeGood(CoreData coreData, CaravanData caravanData, TraderStatData statData, long tradeGoodId)
        {
            AddTradeGoodToCaravanResult result = new AddTradeGoodToCaravanResult()
            {
                Success = false,
                Travel = _caravanService.GetTravelInfo(coreData),
            };

            CaravanPosition position = _caravanService.GetPosition(coreData);

            if (position.GetCurrentCity() == null)
            {
                result.ErrorMessage = "You can only buy trade goods in cities.";
                return result;
            }

            CityTradeGood tradeGood = position.GetCurrentCity().TradeGoodsProduced.FirstOrDefault(x => x.TradeGoodId == tradeGoodId);

            if (tradeGood == null)
            {
                result.ErrorMessage = "This city doesn't produce that trade good.";
                return result;
            }

            long buyCost = position.GetCurrentCity().TradeGoodBuyCosts[tradeGoodId];


            if (buyCost == 0)
            {
                result.ErrorMessage = "This item cannot be bought in this city.";
                return result;
            }

            if (buyCost > coreData.Currencies[CoreCurrencyTypes.Coins])
            {
                result.ErrorMessage = "You don't have enough money to buy this.";
                return result;
            }

            result.BuyCost = buyCost;

            coreData.Currencies.Add(CoreCurrencyTypes.Coins, -buyCost);

            result = AddTradeGoodToCaravan(coreData, caravanData, statData, tradeGoodId);
            result.BuyCost = buyCost;
            return result;
        }

        public AddTradeGoodToCaravanResult AddTradeGoodToCaravan(CoreData coreData, CaravanData caravanData, TraderStatData statData, long tradeGoodId)
        {

            AddTradeGoodToCaravanResult result = new AddTradeGoodToCaravanResult()
            {
                Success = false,
                Travel = _caravanService.GetTravelInfo(coreData),
            };
            caravanData.TradeGoods.Add(new CaravanTradeGood() { TradeGoodId = tradeGoodId });

            _caravanService.UpdateTravelStatsFromCaravan(coreData, caravanData, statData);

            result.TradeGoodId = tradeGoodId;
            result.Travel = _caravanService.GetTravelInfo(coreData);

            result.Success = true;

            return result;
        }

        public RemoveTradeGoodFromCaravanResult SellTradeGood(CoreData coreData, CaravanData caravanData, TraderStatData statData, long tradeGoodId)
        {
            RemoveTradeGoodFromCaravanResult result = new RemoveTradeGoodFromCaravanResult()
            {
                Success = false,
                Travel = _caravanService.GetTravelInfo(coreData),
            };


            CaravanPosition position = _caravanService.GetPosition(coreData);

            if (position.GetCurrentCity() == null)
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

            TradeEconomySettings econSettings = _gameData.Get<TradeEconomySettings>(coreData);

            long sellValue = (long)(position.GetCurrentCity().TradeGoodBuyCosts[tradeGoodId] * econSettings.SellPricePercent);

            coreData.Currencies.Add(CoreCurrencyTypes.Coins, sellValue);

            result = RemoveTradeGoodFromCaravan(coreData, caravanData, statData, tradeGoodId);

            result.SellValue = sellValue;
            return result;

        }


        public RemoveTradeGoodFromCaravanResult RemoveTradeGoodFromCaravan(CoreData coreData, CaravanData caravanData, TraderStatData statData, long tradeGoodId)
        {
            RemoveTradeGoodFromCaravanResult result = new RemoveTradeGoodFromCaravanResult()
            {
                Success = false,
                Travel = _caravanService.GetTravelInfo(coreData),
            };

            CaravanTradeGood tradeGood = caravanData.TradeGoods.FirstOrDefault(x => x.TradeGoodId == tradeGoodId);

            if (tradeGood == null)
            {
                result.ErrorMessage = "You don't have that item.";
                return result;
            }


            caravanData.TradeGoods.Remove(tradeGood);

            _caravanService.UpdateTravelStatsFromCaravan(coreData, caravanData, statData);

            result.TradeGoodId = tradeGoodId;
            result.Travel = _caravanService.GetTravelInfo(coreData);

            result.Success = true;

            return result;
        }
    }
}



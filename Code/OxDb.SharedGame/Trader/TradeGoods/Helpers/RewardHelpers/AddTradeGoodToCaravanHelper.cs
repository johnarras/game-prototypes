using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Rewards.Interfaces;
using OxDb.SharedGame.Trader.Caravans.PlayerData;
using OxDb.SharedGame.Trader.TradeGoods.Services;
using OxDb.SharedGame.Trader.TradeGoods.WebApi;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.TradeGoods.Helpers.RewardHelpers
{
    public class AddTradeGoodToCaravanHelper : IRewardHelper
    {

        private ITradeGoodService _tradeGoodService = null;
        public long HelperKey => EntityTypes.TradeGood;

        public async Task<long> GetQuantity(IUnitDataLookup lookup, long entityId)
        {
            CaravanData caravanData = await lookup.GetAsync<CaravanData>();

            return caravanData.TradeGoods.Count(x => x.TradeGoodId == entityId);
        }

        public async Task<bool> GiveReward(IUnitDataLookup lookup, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp)
        {
            AddTradeGoodToCaravanResponse result = await _tradeGoodService.AddTradeGoodToCaravan(lookup, entityId, uniqueId);

            return result.Success;
        }
    }
}

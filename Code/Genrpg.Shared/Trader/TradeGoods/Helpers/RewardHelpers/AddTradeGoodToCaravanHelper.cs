using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Interfaces;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.TradeGoods.Services;
using Genrpg.Shared.Trader.TradeGoods.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.TradeGoods.Helpers.RewardHelpers
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

        public async Task<bool> GiveReward(IUnitDataLookup lookup, long entityId, long quantity, object extraData, RewardParams rp)
        {
            AddTradeGoodToCaravanResponse result = await _tradeGoodService.AddTradeGoodToCaravan(lookup, entityId);

            if (result.Success)
            {
                //lookup.AddResponse(result);
            }
            return result.Success;
        }
    }
}

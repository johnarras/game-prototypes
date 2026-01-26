using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.RewardHelpers.Core;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Stats.PlayerData;

namespace Genrpg.RequestServer.Trader.Stats.RewardHelpers
{

    // These are all in the same file since they are so closely linked, I want to be
    // able to inspect them all or change them all at once.


    /// <summary>
    /// Set current CoreCurrency quantity.
    /// </summary>
    public class BaseTraderStatAsyncRewardHelper : BaseAsyncRewardHelper
    {
        public override long HelperKey => EntityTypes.BaseTraderStat;

        public override async Task GiveRewardsAsync(WebContext context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            TraderStatData statData = await context.GetAsync<TraderStatData>();

            if (statData.Stats[entityId].Base < quantity)
            {
                statData.Stats[entityId].Base = (int)quantity;
            }
        }
    }
}



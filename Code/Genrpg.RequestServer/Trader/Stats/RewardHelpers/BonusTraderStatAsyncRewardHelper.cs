using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.RewardHelpers.Core;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Stats.Constants;

namespace Genrpg.RequestServer.CoreCurrencies.RewardHelpers
{

    // These are all in the same file since they are so closely linked, I want to be
    // able to inspect them all or change them all at once.


    /// <summary>
    /// Set current CoreCurrency quantity.
    /// </summary>
    public class BonusTraderStatAsyncRewardHelper : BaseAsyncRewardHelper
    {
        public override long HelperKey => EntityTypes.BonusTraderStat;

        public override async Task GiveRewardsAsync(WebContext context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            TraderStatData statData = await context.GetAsync<TraderStatData>();

            statData.Stats.Get(entityId).AddBonusValue(quantity);

            if (entityId == TraderStats.Foraging)
            {
                context.user.Foraging = statData.Stats.Get(entityId).Max();
            }
        }
    }
}

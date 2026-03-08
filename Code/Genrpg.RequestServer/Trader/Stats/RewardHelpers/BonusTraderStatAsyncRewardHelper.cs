using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.RewardHelpers.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Stats.Services;

namespace Genrpg.RequestServer.Trader.Stats.RewardHelpers
{

    // These are all in the same file since they are so closely linked, I want to be
    // able to inspect them all or change them all at once.


    /// <summary>
    /// Set current CoreCurrency quantity.
    /// </summary>
    public class BonusTraderStatAsyncRewardHelper : BaseAsyncRewardHelper
    {

        private ITraderStatService _statService = null;

        public override long HelperKey => EntityTypes.TraderBonusStat;

        public override async Task<long> GetQuantityAsync(WebContext context, long entityId)
        {
            return _statService.GetBonusStat(await context.GetAsync<TraderStatData>(), entityId);
        }

        public override async Task<bool> GiveRewardAsync(WebContext context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            _statService.AddBonusStat(
                await context.GetAsync<CoreData>(),
                await context.GetAsync<CaravanData>(),
                await context.GetAsync<TraderStatData>(),
                entityId, quantity
                );

            return true;
        }
    }
}



using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Holdings.PlayerData;

namespace Genrpg.RequestServer.Trader.Stats.RewardHelpers
{
    public class AnimalAsyncRewardHelper : IAsyncRewardHelper
    {
        public long HelperKey => EntityTypes.Animal;

        public async Task GiveRewardsAsync(WebContext context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            HoldingsData holdings = await context.GetAsync<HoldingsData>();

            holdings.AnimalsOwned.SetBit(entityId);
        }
    }
}

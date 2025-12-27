using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Interfaces;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.RequestServer.Rewards.Services
{
    public class WebRewardService : IWebRewardService
    {

        private SetupDictionaryContainer<long, IAsyncRewardHelper> _rewardHelpers = new SetupDictionaryContainer<long, IAsyncRewardHelper>();
        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private IAsyncRewardHelper GetRewardHelper(long rewardTypeId)
        {
            if (_rewardHelpers.TryGetValue(rewardTypeId, out IAsyncRewardHelper rewardHelper))
            {
                return rewardHelper;
            }
            return null;
        }

        public async Task GiveRewardsAsync(WebContext context, List<Reward> rewards, RewardParams rp)
        {
            if (rp == null)
            {
                rp = new RewardParams();
            }
            foreach (Reward reward in rewards)
            {
                IAsyncRewardHelper helper = GetRewardHelper(reward.EntityTypeId);
                if (helper != null)
                {
                    await helper.GiveRewardsAsync(context, reward.EntityId, reward.Quantity, reward.EntityId, rp);
                }
            }
        }

        public async Task GiveRewardsAsync(WebContext context, List<RewardList> rewardLists, RewardParams rp)
        {
            foreach (RewardList rewardList in rewardLists)
            {
                await GiveRewardsAsync(context, rewardList.Rewards, rp);
            }
        }
    }
}



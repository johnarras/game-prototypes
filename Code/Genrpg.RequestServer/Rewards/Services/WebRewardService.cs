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

        public async Task<bool> GiveRewardsAsync<IR>(WebContext context, List<IR> rewards, RewardParams rp) where IR : IReward
        {
            if (rp == null)
            {
                rp = new RewardParams();
            }
            bool allSuccess = true;
            foreach (IR reward in rewards)
            {
                if (!await GiveRewardAsync(context, reward, rp))
                {
                    allSuccess = false;
                }
            }
            return allSuccess;
        }

        public async Task<bool> GiveRewardsAsync(WebContext context, List<RewardList> rewardLists, RewardParams rp)
        {
            bool allSuccess = true;
            foreach (RewardList rewardList in rewardLists)
            {
                if (!await GiveRewardsAsync(context, rewardList.Rewards, rp))
                {
                    allSuccess = false; 
                }
            }
            return allSuccess;
        }

        public async Task<bool> GiveRewardAsync<IR>(WebContext context, IR rew, RewardParams rp) where IR : IReward
        {

            IAsyncRewardHelper helper = GetRewardHelper(rew.EntityTypeId);
            if (helper != null)
            {
                return await helper.GiveRewardAsync(context, rew.EntityId, rew.Quantity, rew.EntityId, rp);
            }
            return false;   
        }
    }
}



using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.RequestServer.Rewards.Services
{
    public interface IWebRewardService : IInitializable
    {

        Task<bool> GiveRewardsAsync(WebContext context, List<RewardList> rewardLists, RewardParams rp);
        Task<bool> GiveRewardsAsync<IR>(WebContext context, List<IR> rewards, RewardParams rp) where IR : IReward;

        Task<bool> GiveRewardAsync<IR>(WebContext context, IR rew, RewardParams rp) where IR : IReward;
    }
}



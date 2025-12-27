using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.RequestServer.Rewards.Services
{
    public interface IWebRewardService : IInitializable
    {

        Task GiveRewardsAsync(WebContext context, List<RewardList> rewardLists, RewardParams rp);
        Task GiveRewardsAsync(WebContext context, List<Reward> rewards, RewardParams rp);
    }
}



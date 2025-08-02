using Genrpg.RequestServer.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Rewards.Services
{
    public interface IWebRewardService : IInitializable
    {

        Task GiveRewardsAsync(WebContext context, List<RewardList> rewardLists, RewardParams rp);
        Task GiveRewardsAsync(WebContext context, List<Reward> rewards, RewardParams rp);

        Task AddQuantity<TChild>(WebContext context, long entityId, long quantity, RewardParams rp) where TChild : class, IOwnerQuantityChild, new();
    }
}

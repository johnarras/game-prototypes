
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Rewards.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.Shared.Rewards.Services
{
    public interface IRewardService : IInitializable
    {
        Task<bool> GiveRewards(IUnitDataLookup context, List<RewardList> rewardLists, RewardParams rp);
        Task<bool> GiveRewards<TReward>(IUnitDataLookup context, List<TReward> rewards, RewardParams rp) where TReward : IEffect;

        Task<bool> GiveReward<TReward>(IUnitDataLookup context, TReward rew, RewardParams rp) where TReward : IEffect;

        Task<bool> GiveReward(IUnitDataLookup context, long entityTypeId, long entityId, long quantity, Item extraData, RewardParams rp);
    }
}



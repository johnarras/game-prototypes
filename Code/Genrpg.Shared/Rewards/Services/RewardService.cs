
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Rewards.Services
{


    public interface IRewardService : IInitializable
    {
        Task<bool> GiveRewards(IUnitDataLookup context, List<RewardList> rewardLists, RewardParams rp);
        Task<bool> GiveRewards<TReward>(IUnitDataLookup context, List<TReward> rewards, RewardParams rp) where TReward : IEffect;

        Task<bool> GiveReward<TReward>(IUnitDataLookup context, TReward rew, RewardParams rp) where TReward : IEffect;

        Task<bool> GiveReward(IUnitDataLookup context, long entityTypeId, long entityId, long quantity, Item extraData, long uniqueId, RewardParams rp);
    }


    public class RewardService : IRewardService
    {
        private SetupDictionaryContainer<long, IRewardHelper> _rewardHelpers = new SetupDictionaryContainer<long, IRewardHelper>();
        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private IRewardHelper GetRewardHelper(long rewardTypeId)
        {
            if (_rewardHelpers.TryGetValue(rewardTypeId, out IRewardHelper rewardHelper))
            {
                return rewardHelper;
            }
            return null;
        }

        public async Task<bool> GiveRewards<TReward>(IUnitDataLookup context, List<TReward> rewards, RewardParams rp) where TReward : IEffect
        {
            if (rp == null)
            {
                rp = new RewardParams();
            }
            bool allSuccess = true;
            foreach (TReward reward in rewards)
            {
                if (!await GiveReward(context, reward, rp))
                {
                    allSuccess = false;
                }
            }
            return allSuccess;
        }

        public async Task<bool> GiveRewards(IUnitDataLookup context, List<RewardList> rewardLists, RewardParams rp)
        {
            bool allSuccess = true;
            foreach (RewardList rewardList in rewardLists)
            {
                if (!await GiveRewards(context, rewardList.Rewards, rp))
                {
                    allSuccess = false;
                }
            }
            return allSuccess;
        }

        public async Task<bool> GiveReward<TReward>(IUnitDataLookup context, TReward rew, RewardParams rp) where TReward : IEffect
        {
            Item extraData = null;
            long uniqueId = 0;
            if (rew is IReward ir)
            {
                extraData = ir.ExtraData;
                uniqueId = ir.UniqueId; 
            }
            return await GiveReward(context, rew.EntityTypeId, rew.EntityId, rew.Quantity, extraData, uniqueId, rp);
        }

        public virtual async Task<bool> GiveReward(IUnitDataLookup context, long entityTypeId, long entityId, long quantity, Item extraData, long uniqueId, RewardParams rp)
        {
            IRewardHelper helper = GetRewardHelper(entityTypeId);
            if (helper != null)
            {
                return await helper.GiveReward(context, entityId, quantity, extraData, uniqueId, rp);
            }
            return false;
        }
    }
}




using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Inventory.PlayerData;

using OxDb.SharedGame.Rewards.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Rewards.Services
{


    public interface IRewardService : IInitializable
    {
        ValueTask<bool> GiveRewards(IUnitDataLookup context, List<RewardList> rewardLists, RewardParams rp);
        ValueTask<bool> GiveRewards<TReward>(IUnitDataLookup context, List<TReward> rewards, long rewardSourceId, RewardParams rp) where TReward : IEffect;

        ValueTask<bool> GiveReward<TReward>(IUnitDataLookup context, TReward rew, long rewardSourceId, RewardParams rp) where TReward : IEffect;

        ValueTask<bool> GiveReward(IUnitDataLookup context, long entityTypeId, long entityId, long quantity, long rewardSourceId, Item extraData, long uniqueId, RewardParams rp);

        RewardList CreateRewardList(long rewardSourceId, long entityId, List<Reward> rewards = null);


        List<RewardList> CreateListFromList(long rewardSourceId, long entityId, List<Reward> rewards = null);

        List<RewardList> CreateListFromReward(long rewardSourceId, long entityId, Reward rew = null);
    }


    public class RewardService : IRewardService
    {
        public RewardList CreateRewardList(long rewardSourceId, long entityId, List<Reward> rewards = null)
        {
            if (rewards == null)
            {
                rewards = new List<Reward>();
            }
            return new RewardList() { RewardSourceId = rewardSourceId, Rewards = rewards, EntityId = entityId };
        }

        public List<RewardList> CreateListFromList(long rewardSourceId, long entityId, List<Reward> rewards)
        {
            return new List<RewardList>() { CreateRewardList(rewardSourceId, entityId, rewards) };
        }

        public List<RewardList> CreateListFromReward(long rewardSourceId,  long entityId, Reward rew = null)
        {

            RewardList rewardList = CreateRewardList(rewardSourceId, entityId);

            if (rew != null)
            {
                rewardList.Rewards.Add(rew);
            }

            return new List<RewardList> { rewardList };

        }

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
            return null!;
        }

        public async ValueTask<bool> GiveRewards<TReward>(IUnitDataLookup context, List<TReward> rewards, long rewardSourceId, RewardParams rp) where TReward : IEffect
        {
            if (rp == null)
            {
                rp = new RewardParams();
            }
            bool allSuccess = true;
            foreach (TReward reward in rewards)
            {
                if (!await GiveReward(context, reward, rewardSourceId, rp))
                {
                    allSuccess = false;
                }
            }
            return allSuccess;
        }

        public async ValueTask<bool> GiveRewards(IUnitDataLookup context, List<RewardList> rewardLists, RewardParams rp)
        {
            bool allSuccess = true;
            foreach (RewardList rewardList in rewardLists)
            {
                if (!await GiveRewards(context, rewardList.Rewards, rewardList.RewardSourceId, rp))
                {
                    allSuccess = false;
                    break;
                }
            }
            return allSuccess;
        }

        public async ValueTask<bool> GiveReward<TReward>(IUnitDataLookup context, TReward rew, long rewardSourceId, RewardParams rp) where TReward : IEffect
        {
            Item extraData = null;
            long uniqueId = 0;
            if (rew is IReward ir)
            {
                extraData = (Item)ir.ExtraData;
                uniqueId = ir.UniqueId;
            }
            return await GiveReward(context, rew.EntityTypeId, rew.EntityId, rew.Quantity, rewardSourceId, extraData, uniqueId, rp);
        }

        public virtual async ValueTask<bool> GiveReward(IUnitDataLookup context, long entityTypeId, long entityId, long quantity, long rewardSourceId, Item extraData, long uniqueId, RewardParams rp)
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



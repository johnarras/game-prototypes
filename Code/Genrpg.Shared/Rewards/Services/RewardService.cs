using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Rewards.Services
{
    public class RewardService : IRewardService
    {


        private SetupDictionaryContainer<long, IRewardHelper> _rewardHelpers = new SetupDictionaryContainer<long, IRewardHelper>();
        protected IRewardHelper GetRewardHelper(long entityTypeId)
        {
            if (_rewardHelpers.TryGetValue(entityTypeId, out IRewardHelper helper))
            {
                return helper;
            }
            return null;
        }

        public virtual bool GiveRewards<RL>(IRandom rand, MapObject obj, List<RL> resultList, RewardParams rp) where RL : RewardList
        {
            if (resultList == null)
            {
                return false;
            }
            bool hadFailure = false;
            if (obj is Character ch)
            {
                foreach (RewardList rl in resultList)
                {
                    foreach (Reward reward in rl.Rewards)
                    {
                        if (!GiveReward(rand, ch, reward, rp))
                        {
                            hadFailure = true;
                        }
                    }
                }
            }
            else
            {
                hadFailure = true;
            }

            return !hadFailure;
        }

        public virtual bool GiveReward(IRandom rand, MapObject obj, IReward reward, RewardParams rp)
        {
            return GiveReward(rand, obj, reward.EntityTypeId, reward.EntityId, reward.Quantity, reward.ExtraData, rp);
        }

        public virtual bool GiveReward(IRandom rand, MapObject obj, long entityType, long entityId, long quantity, object extraData, RewardParams rp)
        {
            IRewardHelper helper = GetRewardHelper(entityType);

            if (helper == null)
            {
                return false;
            }

            // This handles any extra results we need to send to the client.
            return helper.GiveReward(rand, obj, entityId, quantity, extraData, rp);
        }

        public virtual long GetQuantity(MapObject obj, long entityTypeId, long entityId)
        {
            IRewardHelper helper = GetRewardHelper(entityTypeId);

            if (helper == null)
            {
                return 0;
            }

            return helper.GetQuantity(obj, entityTypeId);
        }

        protected virtual void OnGiveRewardSuccess(IRandom rand, MapObject obj, long entityTypeId, long entityId, long quantity, object extraData, RewardParams rp)
        {

        }
    }
}



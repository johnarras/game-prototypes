using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Rewards.Services
{
    public class RewardService : IRewardService
    {

        protected IRepositoryService _repoService;

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

        public virtual bool GiveReward(IRandom rand, MapObject obj, Reward res, RewardParams rp)
        {
            return GiveReward(rand, obj, res.EntityTypeId, res.EntityId, res.Quantity, res.ExtraData, rp);
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

        public bool Add(MapObject obj, long entityTypeId, long entityId, long quantity, RewardParams rp)
        {
            IQuantityRewardHelper quantityHelper = GetRewardHelper(entityTypeId) as IQuantityRewardHelper;
            if (quantityHelper != null)
            {
                return quantityHelper.Add(obj, entityId, quantity,rp);
            }
            return false;
        }

        public bool Set(MapObject obj, long entityTypeId, long entityId, long quantity, RewardParams rp)
        {
            IQuantityRewardHelper quantityHelper = GetRewardHelper(entityTypeId) as IQuantityRewardHelper;
            if (quantityHelper != null)
            {
                return quantityHelper.Set(obj, entityId, quantity, rp);
            }
            return false;
        }

        public virtual void OnAddQuantity<TUpd>(MapObject obj, TUpd upd, long entityTypeId, long entityId, long diff, RewardParams rp) where TUpd: class, IStringId
        {
        }
    }
}

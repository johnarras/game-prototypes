using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Settings.Qualities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Rewards.Helpers
{
    public abstract class BaseQuantityRewardHelper<TParent, TChild> : IQuantityRewardHelper where TParent : OwnerQuantityObjectList<TChild>, new() where TChild : OwnerQuantityChild, IId, new()
    {

        protected IRewardService _rewardService;
        public abstract long Key { get; }


        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
           return Add(obj, entityId, quantity, rp);
        }

        public bool Add(MapObject obj, long entityId, long quantity, RewardParams rp)
        {
            if (quantity == 0)
            {
                return false;
            }

            TParent parentData = obj.Get<TParent>();
            TChild status = parentData.Get(entityId);
            status.Quantity += quantity;
            _rewardService.OnAddQuantity(obj, status, Key, status.IdKey, quantity, rp);
            return true;
        }

        public long Get(MapObject obj, long entityId)
        {
            return obj.Get<TParent>().Get(entityId).Quantity;
        }

        public bool Set(MapObject obj, long entityId, long quantity, RewardParams rp)
        {
            TParent parentData = obj.Get<TParent>();
            TChild status = parentData.Get(entityId);
            long oldQuantity = Math.Max(0, status.Quantity);
            long diff = quantity - oldQuantity;
            Add(obj, entityId, diff, rp);
            return true;
        }
    }
}

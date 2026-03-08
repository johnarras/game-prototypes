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
    public abstract class BaseQuantityRewardHelper<TParent, TChild> where TParent : OwnerQuantityObjectList<TChild>, new() where TChild : OwnerQuantityChild, IId, new()
    {
        public abstract long HelperKey { get; }


        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
            TParent parentData = obj.Get<TParent>();
            TChild status = parentData.Get(entityId);
            status.Quantity += quantity;

            if (status.Quantity < 0)
            {
                status.Quantity = 0;
            }
            return true;
        }
        public long Get(MapObject obj, long entityId)
        {
            return obj.Get<TParent>().Get(entityId).Quantity;
        }
    }
}



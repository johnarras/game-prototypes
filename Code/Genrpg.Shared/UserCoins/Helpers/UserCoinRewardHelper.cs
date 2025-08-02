using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.UserCoins.Helpers
{

    public class UserCoinRewardHelper : IQuantityRewardHelper
    {

        private IRewardService _rewardService = null!;


        public bool Set(MapObject obj, long entityId, long quantity, RewardParams rp)
        {
            obj.Get<CoreUserData>().Coins.Set(entityId, quantity);
            return true;
        }
        public bool Add(MapObject obj, long entityId, long quantity, RewardParams rp)
        {
            return Set(obj, entityId, Get(obj, entityId) + quantity, rp);
        }

        public long Get(MapObject obj, long entityId)
        {
            return obj.Get<CoreUserData>().Coins.Get(entityId);
        }

        public long Key => EntityTypes.UserCoin;

        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
            if (quantity == 0)
            {
                return false;
            }

            CoreUserData userData = obj.Get<CoreUserData>();
            userData.Coins.Add(entityId, quantity);

            _rewardService.OnAddQuantity(obj, userData, Key, entityId, quantity, rp);

            return true;
        }
    }
}

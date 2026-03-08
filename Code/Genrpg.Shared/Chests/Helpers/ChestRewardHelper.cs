using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Helpers;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Chests.PlayerData;

namespace Genrpg.Shared.Chests.Helpers
{
    public class ChestRewardHelper : IRewardHelper
    {
        public long HelperKey => EntityTypes.Chest;

        public long GetQuantity(MapObject obj, long entityId)
        {
            return 0;
        }

        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
            
            // Need to get loot from chest and give to player.
            return true;
        }
    }
}



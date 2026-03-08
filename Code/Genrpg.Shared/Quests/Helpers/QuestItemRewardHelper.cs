
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Utils;
namespace Genrpg.Shared.Quests.Helpers
{
    public class QuestItemRewardHelper : IRewardHelper
    {
        public long HelperKey => EntityTypes.QuestItem;

        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
            if (quantity < 1)
            {
                return false;
            }

            return true;
        }

        public long GetQuantity(MapObject obj, long entityId)
        {
            return 0;
        }

    }
}



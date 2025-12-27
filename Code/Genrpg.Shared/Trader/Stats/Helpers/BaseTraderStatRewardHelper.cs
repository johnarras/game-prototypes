using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Trader.Stats.Constants;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Trader.Stats.Helpers
{
    public class BaseTraderRewardHelper : IRewardHelper
    {
        public long HelperKey => EntityTypes.BaseTraderStat;

        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
            TraderStatData statData = obj.Get<TraderStatData>();

            statData.Stats.Get(entityId).Base = quantity;

            CoreUserData userData = obj.Get<CoreUserData>();

            if (entityId == TraderStats.Foraging)
            {
                userData.Foraging = statData.Stats.Get(entityId).Max();
            }

            return true;
        }
    }
}



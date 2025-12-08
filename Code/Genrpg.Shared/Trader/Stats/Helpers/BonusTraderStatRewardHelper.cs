using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Trader.Stats.Constants;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Currencies.Helpers
{
    public class BonusTraderRewardHelper : IRewardHelper
    {
        public long HelperKey => EntityTypes.BonusTraderStat;

        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
            TraderStatData statData = obj.Get<TraderStatData>();

            statData.Stats.Get(entityId).AddBonusValue(quantity);

            CoreUserData userData = obj.Get<CoreUserData>();

            if (entityId == TraderStats.Foraging)
            {
                userData.Foraging = statData.Stats.Get(entityId).Max();
            }

            return true;
        }
    }
}

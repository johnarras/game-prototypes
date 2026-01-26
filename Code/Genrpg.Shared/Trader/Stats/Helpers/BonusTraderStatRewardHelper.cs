using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Trader.Stats.Helpers
{
    public class BonusTraderRewardHelper : IRewardHelper
    {
        public long HelperKey => EntityTypes.BonusTraderStat;

        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
            TraderStatData statData = obj.Get<TraderStatData>();

            statData.Stats[entityId].Bonus += (int)quantity;

            CoreData coreData = obj.Get<CoreData>();

            return true;
        }
    }
}



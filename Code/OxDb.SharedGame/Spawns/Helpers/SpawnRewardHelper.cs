
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;

using OxDb.SharedGame.Rewards.Interfaces;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Spawns.Helpers
{
    public class SpawnRewardHelper : IRewardHelper
    {
        public long HelperKey => EntityTypes.Spawn;


        public async ValueTask<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            return 0;
        }


        public async ValueTask<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp)
        {
            return true;
        }
    }
}



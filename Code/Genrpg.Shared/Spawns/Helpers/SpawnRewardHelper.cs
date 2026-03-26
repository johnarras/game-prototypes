
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.Shared.Spawns.Helpers
{
    public class SpawnRewardHelper : IRewardHelper
    {
        public long HelperKey => EntityTypes.Spawn;


        public async Task<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            await Task.CompletedTask;
            return 0;
        }


        public async Task<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            await Task.CompletedTask;
            return true;
        }
    }
}



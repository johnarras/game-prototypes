using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.Shared.Spells.Helpers
{

    public class SpellRewardHelper : IRewardHelper
    {
        public long HelperKey => EntityTypes.Spell;

        public async Task<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            await Task.CompletedTask;
            return 0;
        }


        public async Task<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp)
        {
            await Task.CompletedTask;
            return true;
        }
    }
}



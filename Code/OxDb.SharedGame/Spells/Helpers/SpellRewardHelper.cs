using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Rewards.Interfaces;
using System.Threading.Tasks;
namespace OxDb.SharedGame.Spells.Helpers
{

    public class SpellRewardHelper : IRewardHelper
    {
        public long HelperKey => EntityTypes.Spell;

        public async Task<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            await System.Threading.Tasks.Task.CompletedTask;
            return 0;
        }


        public async Task<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp)
        {
            await System.Threading.Tasks.Task.CompletedTask;
            return true;
        }
    }
}



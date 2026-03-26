using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.Shared.Attributes.Helpers
{
    public class GameplayBuffRewardHelper : IRewardHelper
    {

        private IAttributeService _atributeService = null;
        public long HelperKey => EntityTypes.GameplayBuff;

        public async Task<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            return await _atributeService.GetBuffSeconds(context, entityId);
        }

        public async Task<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, RewardParams rp)
        {

            await _atributeService.AddBuff(context, entityId, quantity);
            return true;
        }
    }
}

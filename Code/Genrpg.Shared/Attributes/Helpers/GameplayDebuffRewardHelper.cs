using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.Shared.Attributes.Helpers
{
    public class GameplayDebuffAsyncRewardHelper : IRewardHelper
    {

        private IAttributeService _attributeService = null;
        public long HelperKey => EntityTypes.GameplayDebuff;

        public async Task<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            return await _attributeService.GetDebuffDays(context, entityId);
        }
        public async Task<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp)
        {

            await _attributeService.AddDebuff(context, entityId, quantity);

            return true;

        }
    }
}

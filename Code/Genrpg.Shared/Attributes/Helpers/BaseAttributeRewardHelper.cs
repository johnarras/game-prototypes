using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.Shared.Attributes.Helpers
{
    public abstract class BaseAttributeRewardHelper : IRewardHelper
    {

        protected IAttributeService _attributeService = null;
        public abstract long HelperKey { get; }

        public virtual async Task<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            return await _attributeService.GetQuantity(context, HelperKey, entityId);
        }

        public virtual async Task<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp)
        {
            return await _attributeService.GiveReward(context, HelperKey, entityId, quantity);
        }
    }
}

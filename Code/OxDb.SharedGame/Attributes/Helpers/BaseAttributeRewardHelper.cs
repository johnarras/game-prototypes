using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Rewards.Interfaces;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Attributes.Helpers
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

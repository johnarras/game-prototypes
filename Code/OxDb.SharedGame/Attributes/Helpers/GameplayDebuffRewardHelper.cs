using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Rewards.Interfaces;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Attributes.Helpers
{
    public class GameplayDebuffAsyncRewardHelper : IRewardHelper
    {

        private IAttributeService _attributeService = null;
        public long HelperKey => EntityTypes.GameplayDebuff;

        public async ValueTask<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            return await _attributeService.GetDebuffDays(context, entityId);
        }
        public async ValueTask<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp)
        {

            await _attributeService.AddDebuff(context, entityId, quantity);

            return true;

        }
    }
}

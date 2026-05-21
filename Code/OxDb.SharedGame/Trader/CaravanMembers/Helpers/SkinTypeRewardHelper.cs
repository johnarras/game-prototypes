using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Rewards.Interfaces;
using OxDb.SharedGame.Trader.CaravanMembers.Services;
using OxDb.SharedGame.Trader.Holdings.PlayerData;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.CaravanMembers.Helpers
{
    public class SkinTypeRewardHelper : IRewardHelper
    {
        private ICaravanMemberService _caravanMemberService = null;

        public long HelperKey => EntityTypes.SkinType;

        public async Task<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            return _caravanMemberService.GetSkinQuantity(await context.GetAsync<HoldingsData>(), entityId);
        }

        public async Task<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp)
        {
            CoreData core = await context.GetAsync<CoreData>();
            _caravanMemberService.AddSkinToHoldings(core, await context.GetAsync<HoldingsData>(), entityId);

            return true;
        }
    }
}

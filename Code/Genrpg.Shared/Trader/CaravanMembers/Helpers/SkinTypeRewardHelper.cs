using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Interfaces;
using Genrpg.Shared.Trader.CaravanMembers.Services;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.CaravanMembers.Helpers
{
    public class SkinTypeRewardHelper : IRewardHelper
    {
        private ICaravanMemberService _caravanMemberService = null;

        public long HelperKey => EntityTypes.SkinType;

        public async Task<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            return _caravanMemberService.GetSkinQuantity(await context.GetAsync<HoldingsData>(), entityId);
        }

        public async Task<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            CoreData core = await context.GetAsync<CoreData>();
            _caravanMemberService.AddSkinToHoldings(core, await context.GetAsync<HoldingsData>(), entityId);

            return true;
        }
    }
}

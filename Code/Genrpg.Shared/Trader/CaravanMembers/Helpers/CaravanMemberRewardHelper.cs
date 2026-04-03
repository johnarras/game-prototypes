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
    public class CaravanMamberRewardHelper : IRewardHelper
    {
        private ICaravanMemberService _CaravanMemberService = null;

        public long HelperKey => EntityTypes.CaravanMember;

        public async Task<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp)
        {
            CoreData core = await context.GetAsync<CoreData>();
            _CaravanMemberService.AddCaravanMemberToHoldings(core, await context.GetAsync<HoldingsData>(), entityId);
            return true;
        }

        public async Task<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            return _CaravanMemberService.GetCaravanMemberQuantity(await context.GetAsync<HoldingsData>(), entityId);
        }
    }
}



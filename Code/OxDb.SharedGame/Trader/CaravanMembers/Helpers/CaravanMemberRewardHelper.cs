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
    public class CaravanMamberRewardHelper : IRewardHelper
    {
        private ICaravanMemberService _caravanMemberService = null;

        public long HelperKey => EntityTypes.CaravanMember;

        public async ValueTask<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp)
        {            
            await _caravanMemberService.AddCaravanMemberToHoldings(context, entityId);
            return true;
        }

        public async ValueTask<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            return await _caravanMemberService.GetCaravanMemberQuantity(context, entityId);
        }
    }
}



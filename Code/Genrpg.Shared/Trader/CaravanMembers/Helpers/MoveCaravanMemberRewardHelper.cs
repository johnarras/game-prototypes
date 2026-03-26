using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Interfaces;
using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Trader.CaravanMembers.Services;
using Genrpg.Shared.Trader.CaravanMembers.WebApi;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.CaravanMembers.Helpers
{
    public class MoveCaravanMemberRewardHelper : IRewardHelper
    {

        protected ICaravanService _caravanService = null;
        public long HelperKey => EntityTypes.MoveCaravanMember;

        public async Task<long> GetQuantity(IUnitDataLookup lookup, long entityId)
        {
            await Task.CompletedTask;
            return 0;
        }

        public async Task<bool> GiveReward(IUnitDataLookup lookup, long entityId, long quantity, object extraData, RewardParams rp)
        {
            CaravanData caravanData = await lookup.GetAsync<CaravanData>();

            CoreData coreData = await lookup.GetAsync<CoreData>();
            HoldingsData holdingsData = await lookup.GetAsync<HoldingsData>(); 
            if (caravanData.CurrentMembers.Any(x => x.CaravanMemberId == entityId))
            {
                RemoveMemberFromCaravanResult result = await _caravanService.RemoveMemberFromCaravan(lookup, entityId, rp?.IsSpendAction ?? false);
                if (result != null && result.Success)
                {
                    lookup.AddResponse(result);
                    return true;
                }
            }
            else
            {
                AddMemberToCaravanResult result = await _caravanService.AddMemberToCaravan(lookup, entityId, rp?.IsSpendAction ?? false);

                if (result != null && result.Success)
                {
                    lookup.AddResponse(result);
                    return true;
                }
            }
            return false;
        }
    }
}

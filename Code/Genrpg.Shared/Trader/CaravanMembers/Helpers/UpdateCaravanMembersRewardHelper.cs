using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Interfaces;
using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Trader.CaravanMembers.Constants;
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
    public class UpdateCaravanMembersRewardHelper : IRewardHelper
    {

        protected ICaravanService _caravanService = null;
        public long HelperKey => EntityTypes.UpdateCaravanMembers;

        public async Task<long> GetQuantity(IUnitDataLookup lookup, long entityId)
        {
            await Task.CompletedTask;
            return 0;
        }

        public async Task<bool> GiveReward(IUnitDataLookup lookup, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp)
        {
            CaravanData caravanData = await lookup.GetAsync<CaravanData>();

            CoreData coreData = await lookup.GetAsync<CoreData>();
            HoldingsData holdingsData = await lookup.GetAsync<HoldingsData>(); 

            
            if (string.IsNullOrEmpty(rp.ExtraRewardArgs))
            {
                return false;
            }

            List<long> memberIds = new List<long>();

            if (rp.ExtraRewardArgs != CaravanMemberConstants.EmptyMemberListString)
            {
                List<string> memberIdNames = rp.ExtraRewardArgs.Split(',').ToList();

                foreach (String mid in memberIdNames)
                {
                    if (Int32.TryParse(mid, out int memberId))
                    {
                        memberIds.Add(memberId);
                    }
                }
            }

            UpdateCaravanMembersResponse result = await _caravanService.UpdateCaravanMembers(lookup, memberIds);

            if (result != null && result.Success)
            {
                lookup.AddResponse(result);
                return true;
            }
            return false;
        }
    }
}

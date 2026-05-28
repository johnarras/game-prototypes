using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Rewards.Interfaces;
using OxDb.SharedGame.Trader.CaravanMembers.Constants;
using OxDb.SharedGame.Trader.CaravanMembers.WebApi;
using OxDb.SharedGame.Trader.Caravans.PlayerData;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Holdings.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.CaravanMembers.Helpers
{
    public class UpdateCaravanMembersRewardHelper : IRewardHelper
    {

        protected ICaravanService _caravanService = null;
        public long HelperKey => EntityTypes.UpdateCaravanMembers;

        public async Task<long> GetQuantity(IUnitDataLookup lookup, long entityId)
        {
            await System.Threading.Tasks.Task.CompletedTask;
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

using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.RewardHelpers.Core;
using System.Threading.Tasks;

namespace Genrpg.Shared.Currencies.RewardHelpers
{

    // These are all in the same file since they are so closely linked, I want to be
    // able to inspect them all or change them all at once.


    /// <summary>
    /// Set current CoreCurrency quantity.
    /// </summary>
    public class CoreCurrencyTypeRewardHelper : BaseRewardHelper
    {
        public override long HelperKey => EntityTypes.CoreCurrency;

        public override async Task<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            CoreData coreData = await context.GetAsync<CoreData>();
            return coreData.Currencies[entityId];
        }

        public override async Task<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            CoreData coreData = await context.GetAsync<CoreData>();

            coreData.Currencies.Add(entityId, quantity);
            return true;
        }
    }
}



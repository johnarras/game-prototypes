using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.RewardHelpers.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.RequestServer.CoreCurrencies.RewardHelpers
{

    // These are all in the same file since they are so closely linked, I want to be
    // able to inspect them all or change them all at once.


    /// <summary>
    /// Set current CoreCurrency quantity.
    /// </summary>
    public class CoreCurrencyAsyncRewardHelper : BaseAsyncRewardHelper
    {
        public override long HelperKey => EntityTypes.CoreCurrency;

        public override async Task<long> GetQuantityAsync(WebContext context, long entityId)
        {
            CoreData coreData = await context.GetAsync<CoreData>();
            return coreData.Currencies[entityId];
        }

        public override async Task<bool> GiveRewardAsync(WebContext context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            CoreData coreData = await context.GetAsync<CoreData>();

            coreData.Currencies.Add(entityId, quantity);
            return true;
        }
    }
}



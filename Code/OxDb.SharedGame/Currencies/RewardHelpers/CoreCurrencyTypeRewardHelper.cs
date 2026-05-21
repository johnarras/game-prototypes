using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Rewards.RewardHelpers.Core;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Currencies.RewardHelpers
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

        public override async Task<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp)
        {
            CoreData coreData = await context.GetAsync<CoreData>();

            coreData.Currencies.Add(entityId, quantity);
            return true;
        }
    }
}



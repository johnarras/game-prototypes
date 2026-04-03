using Genrpg.Shared.Currencies.PlayerData;
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
    public class CharCurrencyTypeRewardHelper : BaseRewardHelper
    {
        public override long HelperKey => EntityTypes.CharCurrency;

        public override async Task<long> GetQuantity(IUnitDataLookup context, long entityId)
        {
            CharCurrencyData currencyData = await context.GetAsync<CharCurrencyData>();
            return currencyData.Data[entityId];
        }

        public override async Task<bool> GiveReward(IUnitDataLookup context, long entityId, long quantity, object extraData, long uniqueId, RewardParams rp)
        {
            CharCurrencyData currencyData = await context.GetAsync<CharCurrencyData>();
            currencyData.Data.Add(entityId, quantity);
            return true;
        }
    }
}



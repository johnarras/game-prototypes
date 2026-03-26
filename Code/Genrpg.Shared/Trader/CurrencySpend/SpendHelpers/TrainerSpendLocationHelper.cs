using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Trader.CurrencySpend.Constants;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.CurrencySpend.SpendHelpers
{
    public class TrainerSpendLocationHelper : BaseSpendLocationHelper
    {
        public override long HelperKey => SpendLocations.Trainer;

        public override async Task<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity)
        {
            List<SpendType> validSpendTypes = new List<SpendType>();

            CoreData coreData = await lookup.GetAsync<CoreData>();

            FullSpendLocation fullSpendloc = new FullSpendLocation()
            {
                Location = GetSpendLocation(coreData)
            };

            fullSpendloc.IsValid = true;
            return fullSpendloc;

        }
    }
}

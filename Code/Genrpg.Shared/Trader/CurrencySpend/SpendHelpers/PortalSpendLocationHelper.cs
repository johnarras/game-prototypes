using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Trader.CurrencySpend.Constants;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using Genrpg.Shared.Trader.Maps.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.CurrencySpend.SpendHelpers
{
    public class PortalSpendLocationHelper : BaseSpendLocationHelper
    {

        private ITraderMapService _mapService = null;
        public override long HelperKey => SpendLocations.Portal;

        public override async Task<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity)
        {
            List<SpendType> validSpendTypes = new List<SpendType>();

            CoreData coreData = await lookup.GetAsync<CoreData>();

            FullSpendLocation fullSpendloc = new FullSpendLocation()
            {
                Location = GetSpendLocation(coreData)
            };

            List<CityTravelDistance> distances = await _mapService.GetNearbyCities(lookup);

            fullSpendloc.SpendTypes.AddRange(distances.Where(x => x.PortalSpend != null).Select(x => x.PortalSpend).ToList());

            fullSpendloc.IsValid = true;
            return fullSpendloc;
        }
    }
}

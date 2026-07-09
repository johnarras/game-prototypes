using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.CurrencySpend.Constants;
using OxDb.SharedGame.Trader.CurrencySpend.Entities;
using OxDb.SharedGame.Trader.CurrencySpend.Settings;
using OxDb.SharedGame.Trader.Maps.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.CurrencySpend.SpendHelpers
{
    public class PortalSpendLocationHelper : BaseSpendLocationHelper
    {

        private ITraderMapService _mapService = null;
        public override long HelperKey => SpendLocations.Portal;

        public override async ValueTask<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity)
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

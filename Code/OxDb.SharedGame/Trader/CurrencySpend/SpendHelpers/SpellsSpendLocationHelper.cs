using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.CurrencySpend.Constants;
using OxDb.SharedGame.Trader.CurrencySpend.Entities;
using OxDb.SharedGame.Trader.CurrencySpend.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.CurrencySpend.SpendHelpers
{
    public class SpellSpendLocationHelper : BaseSpendLocationHelper
    {
        public override long HelperKey => SpendLocations.Spells;

        public override async Task<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity)
        {
            List<SpendType> validSpendTypes = new List<SpendType>();

            CoreData coreData = await lookup.GetAsync<CoreData>();

            FullSpendLocation fullSpendloc = new FullSpendLocation()
            {
                Location = GetSpendLocation(coreData)
            };


            foreach (SpendType stype in fullSpendloc.Location.SpendTypes)
            {
                if (stype.MinLevel > coreData.Level)
                {
                    continue;
                }
                fullSpendloc.SpendTypes.Add(stype);
            }

            fullSpendloc.IsValid = true;
            return fullSpendloc;

        }
    }
}

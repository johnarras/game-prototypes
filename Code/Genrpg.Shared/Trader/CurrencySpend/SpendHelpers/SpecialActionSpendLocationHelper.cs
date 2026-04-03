using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.CaravanMembers.Settings;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Constants;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.CurrencySpend.SpendHelpers
{
    public class SpecialActionSpendLocationHelper : BaseSpendLocationHelper
    {
        public override long HelperKey => SpendLocations.SpecialActions;

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

                

                if (stype.Rewards.Any(x=>x.EntityTypeId == EntityTypes.UpdateCaravanMembers))
                {
                    CaravanPosition pos = _caravanService.GetPosition(coreData);

                    // Can do this in one step with != instead of && but it's a bit confusing
                    if (pos.GetCurrentCity() != null && stype.Name.ToLower().Contains("city"))
                    {
                        fullSpendloc.SpendTypes.Add(stype);
                    }
                    else if (pos.GetCurrentCity() == null && !stype.Name.ToLower().Contains("city"))
                    {
                        fullSpendloc.SpendTypes.Add(stype);
                    }
                }
                else
                {
                    fullSpendloc.SpendTypes.Add(stype);

                }

            }

      


            fullSpendloc.IsValid = true;
            return fullSpendloc;

        }
    }
}

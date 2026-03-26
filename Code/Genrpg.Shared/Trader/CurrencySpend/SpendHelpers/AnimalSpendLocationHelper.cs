using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.CaravanMembers.Settings;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.CurrencySpend.Constants;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.CurrencySpend.SpendHelpers
{
    public class CaravanMemberSpendLocationHelper : BaseSpendLocationHelper
    {
        public override long HelperKey => SpendLocations.CaravanMembers;

        public override async Task<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity)
        {
            List<SpendType> validSpendTypes = new List<SpendType>();

            CoreData coreData = await lookup.GetAsync<CoreData>();

            SpendLocation loc = GetSpendLocation(coreData);

            FullSpendLocation fullSpendLoc = new FullSpendLocation()
            {
                Location = GetSpendLocation(coreData)
            };

            CaravanMemberSettings CaravanMemberSettings = _gameData.Get<CaravanMemberSettings>(coreData);

            IReadOnlyList<CaravanMember> allCaravanMembers = CaravanMemberSettings.GetData();

            CaravanPosition pos = _caravanService.GetPosition(coreData);

            List<CityCaravanMember> currCaravanMembers = new List<CityCaravanMember>();

            City city = pos.GetCurrentCity();

            if (useCurrentCity)
            {
                if (city == null)
                {
                    return fullSpendLoc;
                }
                else
                {
                    currCaravanMembers = city.CaravanMembersForSale.ToList();
                }
            }
            else
            {
                foreach (CaravanMember atype in allCaravanMembers)
                {
                    currCaravanMembers.Add(new CityCaravanMember()
                    {
                        CaravanMemberId = atype.IdKey,
                    });
                }
            }

            fullSpendLoc.IsValid = true;

            HoldingsData holdings = await lookup.GetAsync<HoldingsData>();

            foreach (CityCaravanMember cityCaravanMember in currCaravanMembers)
            {
                CaravanMember atype = CaravanMemberSettings.Get(cityCaravanMember.CaravanMemberId);

                if (atype == null)
                {
                    continue;
                }

                if (holdings.CaravanMembersOwned.HasBitIndex(cityCaravanMember.CaravanMemberId))
                {
                    continue;
                }

                SpendType stype = new SpendType()
                {
                    SpendCoreCurrencyTypeId = CoreCurrencyTypes.Coins,
                    SpendQuantity = atype.Price,
                    Index = atype.IdKey,
                    Name = atype.Name,
                    Desc = atype.Desc,
                    MaxTimes = 1,
                    MinLevel = 1,
                };

                stype.Rewards.Add(new SpendReward()
                {
                    EntityTypeId = EntityTypes.CaravanMember,
                    EntityId = atype.IdKey,
                    Quantity = 1,
                });

                fullSpendLoc.SpendTypes.Add(stype);
            }

            return fullSpendLoc;
        }
    }
}

using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.CaravanMembers.Settings;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Cities.Settings;
using OxDb.SharedGame.Trader.CurrencySpend.Constants;
using OxDb.SharedGame.Trader.CurrencySpend.Entities;
using OxDb.SharedGame.Trader.CurrencySpend.Settings;
using OxDb.SharedGame.Trader.Holdings.PlayerData;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.CurrencySpend.SpendHelpers
{
    public class CaravanMemberSpendLocationHelper : BaseSpendLocationHelper
    {
        public override long HelperKey => SpendLocations.CaravanMembers;

        public override async ValueTask<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity)
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

            CaravanPosition pos = await _caravanService.GetPosition(lookup);

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

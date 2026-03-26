using Genrpg.Shared.Attributes.PlayerData;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.CurrencySpend.Constants;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.CurrencySpend.SpendHelpers
{
    public class SuppliesSpendLocationHelper : BaseSpendLocationHelper
    {

        protected ICoreCurrencyService _coreCurrencyService = null;

        public override long HelperKey => SpendLocations.Supplies;

        public override async Task<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity)
        {
            List<SpendType> validSpendTypes = new List<SpendType>();

            CoreData coreData = await lookup.GetAsync<CoreData>();

            SpendLocation loc = GetSpendLocation(coreData);

            FullSpendLocation fullSpendLoc = new FullSpendLocation()
            {
                Location = GetSpendLocation(coreData)
            };

            CaravanPosition pos = _caravanService.GetPosition(coreData);

            if (pos.GetCurrentCity() == null)
            {
                return fullSpendLoc;
            }

            City city = pos.GetCurrentCity();


            AttributeData attributeData = await lookup.GetAsync<AttributeData>();

            CoreCurrencyTypeSettings currencySettings = _gameData.Get<CoreCurrencyTypeSettings>(coreData);

            foreach (SpendType stype in fullSpendLoc.Location.SpendTypes)
            {
                if (stype.SpendCoreCurrencyTypeId != CoreCurrencyTypes.Coins || stype.SpendQuantity < 1)
                {
                    continue;
                }

                if (stype.Rewards.Count != 1)
                {
                    continue;
                }

                SpendReward reward = stype.Rewards[0];

                if (reward.EntityTypeId != EntityTypes.CoreCurrency || reward.EntityId == CoreCurrencyTypes.Coins)
                {
                    continue;
                }

                CoreCurrencyType outputCurrency = currencySettings.Get(reward.EntityId);

                if (outputCurrency == null)
                {
                    continue;
                }

                long currVal = coreData.Currencies[outputCurrency.IdKey];

                long maxVal = await _coreCurrencyService.GetStorage(lookup, outputCurrency.IdKey);

                if (currVal >= maxVal)
                {
                    continue;
                }

                long diff = maxVal - currVal;

                long totalCost = diff * stype.SpendQuantity;

                SpendType newSpend = new SpendType()
                {
                    SpendCoreCurrencyTypeId = stype.SpendCoreCurrencyTypeId,
                    SpendQuantity = totalCost,
                    Desc = stype.Desc,
                    Name = stype.Name,
                    Index = outputCurrency.IdKey,

                };

                newSpend.Rewards.Add(new SpendReward()
                {
                    EntityTypeId = EntityTypes.CoreCurrency,
                    EntityId = outputCurrency.IdKey,
                    Quantity = diff,
                });

                fullSpendLoc.SpendTypes.Add(newSpend);
            }

            fullSpendLoc.IsValid = true;

            return fullSpendLoc;

        }
    }
}

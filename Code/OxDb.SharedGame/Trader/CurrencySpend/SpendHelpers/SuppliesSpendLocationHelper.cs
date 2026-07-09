using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Attributes.PlayerData;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Currencies.Services;
using OxDb.SharedGame.Currencies.Settings;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Cities.Settings;
using OxDb.SharedGame.Trader.CurrencySpend.Constants;
using OxDb.SharedGame.Trader.CurrencySpend.Entities;
using OxDb.SharedGame.Trader.CurrencySpend.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.CurrencySpend.SpendHelpers
{
    public class SuppliesSpendLocationHelper : BaseSpendLocationHelper
    {

        protected ICoreCurrencyService _coreCurrencyService = null;

        public override long HelperKey => SpendLocations.Supplies;

        public override async ValueTask<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity)
        {
            List<SpendType> validSpendTypes = new List<SpendType>();

            CoreData coreData = await lookup.GetAsync<CoreData>();

            SpendLocation loc = GetSpendLocation(coreData);

            FullSpendLocation fullSpendLoc = new FullSpendLocation()
            {
                Location = GetSpendLocation(coreData)
            };

            CaravanPosition pos = await _caravanService.GetPosition(lookup);

            if (pos.GetCurrentCity() == null)
            {
                return fullSpendLoc;
            }

            City city = pos.GetCurrentCity();


            AttributesData attributeData = await lookup.GetAsync<AttributesData>();

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

using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Attributes.PlayerData;
using OxDb.SharedGame.Attributes.Settings;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.Constants;
using OxDb.SharedGame.Trader.CurrencySpend.Constants;
using OxDb.SharedGame.Trader.CurrencySpend.Entities;
using OxDb.SharedGame.Trader.CurrencySpend.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.CurrencySpend.SpendHelpers
{
    public class TempleSpendLocationHelper : BaseSpendLocationHelper
    {
        public override long HelperKey => SpendLocations.Temple;

        protected virtual string FixString => "Cure";

        public override async Task<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity)
        {
            List<SpendType> validSpendTypes = new List<SpendType>();

            CoreData coreData = await lookup.GetAsync<CoreData>();

            FullSpendLocation fullSpendloc = new FullSpendLocation()
            {
                Location = GetSpendLocation(coreData)
            };

            SpendType genericSpend = GetSpendTypeWithReward(coreData, EntityTypes.GameplayDebuff, 0);

            long currDebuffDay = coreData.Vars[TraderVars.DebuffDaysPlayed];

            AttributesData attributeData = await lookup.GetAsync<AttributesData>();

            IReadOnlyList<GameplayDebuff> debuffs = _gameData.Get<GameplayDebuffSettings>(coreData).GetData();

            foreach (GameplayDebuff debuff in debuffs)
            {
                if (attributeData.Debuffs[debuff.IdKey].EndDebuffPlayCount <= currDebuffDay)
                {
                    continue;
                }

                long diff = attributeData.Debuffs[debuff.IdKey].EndDebuffPlayCount - currDebuffDay;

                SpendType specificSpend = GetSpendTypeWithReward(coreData, EntityTypes.GameplayDebuff, debuff.IdKey);

                SpendType finalSpend = specificSpend;

                if (finalSpend == null)
                {
                    finalSpend = genericSpend;
                }

                if (finalSpend == null || finalSpend.SpendQuantity < 1 ||
                    finalSpend.SpendCoreCurrencyTypeId != CoreCurrencyTypes.Coins ||
                    finalSpend.MinLevel > coreData.Level)
                {
                    continue;
                }

                SpendType newSpend = new SpendType()
                {
                    Name = FixString + " " + debuff.Name,
                    Desc = debuff.Name + " ends in " + diff + " day" + (diff == 1 ? "" : "s") + ".",
                    SpendCoreCurrencyTypeId = finalSpend.SpendCoreCurrencyTypeId,
                    SpendQuantity = finalSpend.SpendQuantity,
                    Index = debuff.IdKey,
                    MaxTimes = 1,
                    MinLevel = 1,
                };

                fullSpendloc.SpendTypes.Add(newSpend);
            }

            fullSpendloc.IsValid = true;
            return fullSpendloc;

        }
    }
}

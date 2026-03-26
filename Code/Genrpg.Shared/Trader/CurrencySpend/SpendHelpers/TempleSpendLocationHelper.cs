using Genrpg.Shared.Attributes.PlayerData;
using Genrpg.Shared.Attributes.Settings;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.CurrencySpend.Constants;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.CurrencySpend.SpendHelpers
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

            AttributeData attributeData = await lookup.GetAsync<AttributeData>();

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

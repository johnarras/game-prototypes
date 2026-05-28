using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Attributes.PlayerData;
using OxDb.SharedGame.Attributes.Settings;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Settings;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.CaravanMembers.Settings;
using OxDb.SharedGame.Trader.Holdings.PlayerData;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Users.Services
{
    public interface IUserSnapshotService : IInjectable
    {
        Task<string> GetSnapshotString(IUnitDataLookup lookup);
    }


    public class UserSnapshotService : IUserSnapshotService
    {

        private IGameData _gameData = null;

        public async Task<string> GetSnapshotString(IUnitDataLookup lookup)
        {
            StringBuilder sb = new StringBuilder();

            CoreData coreData = await lookup.GetAsync<CoreData>();

            sb.Append("[Currencies: ");

            IReadOnlyList<CoreCurrencyType> allCurrencies = _gameData.Get<CoreCurrencyTypeSettings>(coreData).GetData();

            int numShown = 0;
            foreach (CoreCurrencyType currencyType in allCurrencies)
            {
                if (coreData.Currencies[currencyType.IdKey] != 0)
                {
                    if (numShown > 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append(" ");
                    sb.Append(currencyType.Name + ": " + coreData.Currencies[currencyType.IdKey]);
                    numShown++;
                }
            }

            sb.Append("] ");


            HoldingsData holdingsData = await lookup.GetAsync<HoldingsData>();

            IReadOnlyList<CaravanMember> members = _gameData.Get<CaravanMemberSettings>(coreData).GetData();

            sb.Append("[Known Members: ");

            numShown = 0;
            foreach (CaravanMember member in members)
            {
                if (holdingsData.CaravanMembersOwned.HasBitIndex(member.IdKey))
                {
                    if (numShown > 0)
                    {
                        sb.Append(",");    
                    }
                    sb.Append(" ");
                    numShown++;
                    sb.Append(member.Name);
                }
            }

            sb.Append("]");


            AttributesData attributesData = await lookup.GetAsync<AttributesData>();

            IReadOnlyList<GameplayStat> allStats = _gameData.Get<GameplayStatSettings>(coreData).GetData();

            AddBonusesToList(sb, "RegenBonus", attributesData.CurrencyRegen, allCurrencies);
            AddBonusesToList(sb, "StorageBonus", attributesData.CurrencyStorage, allCurrencies);
            AddBonusesToList(sb, "StatBonus", attributesData.Stats, allStats);

            return sb.ToString();
          
        }


        private void AddBonusesToList<IDN>(StringBuilder sb, string prefix, AttributeCollection collection, IReadOnlyList<IDN> gameplayItems) where IDN : IIdName
        {
            sb.Append("[" + prefix);

            int numShown = 0;
            foreach (IDN idn in gameplayItems)
            {
                if (collection[idn.IdKey] != null && collection[idn.IdKey].Bonus != 0)
                {
                    if (numShown > 0)
                    {
                        sb.Append(",");
                    }                  
                    sb.Append(" ");
                    numShown++;
                    sb.Append(idn.Name + " {" + collection[idn.IdKey].Bonus + "}");
                }
            }

            sb.Append("]");
        }
       
    }
}

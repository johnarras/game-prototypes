using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.CurrencySpend.Entities;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.CurrencySpend.SpendHelpers
{
    public interface ISpendLocationHelper : ISetupDictionaryItem<long>
    {
        ValueTask<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity);
    }
}

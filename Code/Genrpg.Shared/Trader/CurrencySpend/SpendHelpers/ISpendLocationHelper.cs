using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.CurrencySpend.SpendHelpers
{
    public interface ISpendLocationHelper : ISetupDictionaryItem<long>
    {
        Task<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, bool useCurrentCity);
    }
}

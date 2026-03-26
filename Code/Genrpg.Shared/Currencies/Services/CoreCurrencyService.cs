using Genrpg.Shared.Attributes.Constants;
using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.Shared.Currencies.Services
{
    public interface ICoreCurrencyService : IInjectable
    {
        Task<long> GetStorage(IUnitDataLookup lookup, long coreCurrencyTypeId);

        Task<long> GetRegen(IUnitDataLookup lookup, long coreCurrencyTypeId);
    }

    public class CoreCurrencyService : ICoreCurrencyService
    {
        protected IGameData _gameData = null;
        protected IAttributeService _attributeService = null;

        public async Task<long> GetRegen(IUnitDataLookup lookup, long coreCurrencyTypeId)
        {
            return await _attributeService.GetQuantity(lookup, EAttributeCategories.CurrencyRegen, EAttributeValIndex.Total, coreCurrencyTypeId);
        }

        public async Task<long> GetStorage(IUnitDataLookup lookup, long coreCurrencyTypeId)
        {
            return await _attributeService.GetQuantity(lookup, EAttributeCategories.CurrencyStorage, EAttributeValIndex.Total, coreCurrencyTypeId);
        }
    }
}



using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Attributes.Constants;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Currencies.Services
{
    public interface ICoreCurrencyService : IInjectable
    {
        ValueTask<long> GetStorage(IUnitDataLookup lookup, long coreCurrencyTypeId);

        ValueTask<long> GetRegen(IUnitDataLookup lookup, long coreCurrencyTypeId);
    }

    public class CoreCurrencyService : ICoreCurrencyService
    {
        protected IGameData _gameData = null;
        protected IAttributeService _attributeService = null;

        public async ValueTask<long> GetRegen(IUnitDataLookup lookup, long coreCurrencyTypeId)
        {
            return await _attributeService.GetQuantity(lookup, EAttributeCategories.CurrencyRegen, EAttributeValIndex.Total, coreCurrencyTypeId);
        }

        public async ValueTask<long> GetStorage(IUnitDataLookup lookup, long coreCurrencyTypeId)
        {
            return await _attributeService.GetQuantity(lookup, EAttributeCategories.CurrencyStorage, EAttributeValIndex.Total, coreCurrencyTypeId);
        }
    }
}



using Genrpg.RequestServer.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.CoreCurrencies.Entities;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayMultiplier.Constants;

namespace Genrpg.RequestServer.CoreCurrencies.Services
{
    public interface IServerCoreCurrencyService : IInjectable
    {
        Task UpdateBaseLimits(WebContext context, bool onLogin);
    }


    public class ServerCoreCurrencyService : IServerCoreCurrencyService
    {
        protected IGameData _gameData = null;

        public async Task UpdateBaseLimits(WebContext context, bool onLogin)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            CoreCurrencyTypeSettings currencySettings = _gameData.Get<CoreCurrencyTypeSettings>(context.user);

            if (userData.Mult == 0)
            {
                userData.Mult = PlayMultConstants.MinMult;
                foreach (CoreCurrencyType currencyType in currencySettings.GetData())
                {
                    userData.Currencies.Set(currencyType.IdKey, CurrencyDataOffset.Curr, currencyType.StartCurr);
                }
            }

            foreach (CoreCurrencyType currencyType in currencySettings.GetData())
            {
                CoreCurrencyStatus status = userData.Currencies.GetStatus(currencyType.IdKey);

                if (status.Get(CurrencyDataOffset.BaseRegen) < currencyType.StartRegen)
                {
                    userData.Currencies.Set(currencyType.IdKey, CurrencyDataOffset.BaseRegen, currencyType.StartRegen);
                }
                if (status.Get(CurrencyDataOffset.BaseStorage) < currencyType.StartStorage)
                {
                    userData.Currencies.Set(currencyType.IdKey, CurrencyDataOffset.BaseStorage, currencyType.StartStorage);
                }
            }
        }
    }
}

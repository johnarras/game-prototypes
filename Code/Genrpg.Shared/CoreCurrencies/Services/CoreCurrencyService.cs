using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Stats.PlayerData;

namespace Genrpg.Shared.CoreCurrencies.Services
{
    public interface ICoreCurrencyService : IInjectable
    {
        long GetStorage(long coreCurrencyId, CoreUserData userData, TraderStatData statData);
        long GetRegen(long coreCurrencyId, CoreUserData userData, TraderStatData statData);
    }

    public class CoreCurrencyService : ICoreCurrencyService
    {
        private IGameData _gameData = null;


        public long GetRegen(long coreCurrencyId, CoreUserData userData, TraderStatData statData)
        {
            CoreCurrencyType ctype = _gameData.Get<CoreCurrencyTypeSettings>(userData).Get(coreCurrencyId);

            if (ctype != null)
            {
                return statData.Stats.Get(ctype.RegenTraderStatId).Max();
            }
            return 0;
        }

        public long GetStorage(long coreCurrencyId, CoreUserData userData, TraderStatData statData)
        {
            CoreCurrencyType ctype = _gameData.Get<CoreCurrencyTypeSettings>(userData).Get(coreCurrencyId);

            if (ctype != null)
            {
                return statData.Stats.Get(ctype.StorageTraderStatId).Max();
            }
            return 0;
        }
    }
}



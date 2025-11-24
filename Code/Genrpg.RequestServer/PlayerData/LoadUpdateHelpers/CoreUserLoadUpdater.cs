
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.CoreCurrencies.Services;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.GameSettings;

namespace Genrpg.RequestServer.PlayerData.LoadUpdateHelpers
{
    public class CoreUserLoadUpdater : IUserLoadUpdater
    {
        private IGameData _gameData = null;
        private IPeriodicUpdateService _periodicUpdateService = null;
        private IServerCoreCurrencyService _coreCurrencyService = null;
        public int Order => 1;

        public Type Key => GetType();


        public async Task Update(WebContext context, List<IUnitData> unitData)
        {

            await _coreCurrencyService.UpdateBaseLimits(context, true);
            await _periodicUpdateService.CheckHourlyCurrencyUpdate(context);
        }
    }
}

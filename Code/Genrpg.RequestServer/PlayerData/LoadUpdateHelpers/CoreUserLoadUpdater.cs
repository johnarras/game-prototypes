
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.RequestServer.Trader.NewPlayer;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.NewPlayers.Settings;

namespace Genrpg.RequestServer.PlayerData.LoadUpdateHelpers
{
    public class CoreUserLoadUpdater : IUserLoadUpdater
    {
        private IGameData _gameData = null;
        private IHourlyUpdateService _periodicUpdateService = null;
        private ITraderNewPlayerService _newPlayerService = null;

        public int Order => 1;

        public Type HelperKey => GetType();

        public async Task Update(WebContext context, List<IUnitData> unitData)
        {
            CoreUserData userData = context.user;

            NewPlayerBonusSettings newPlayerSettings = _gameData.Get<NewPlayerBonusSettings>(context.user);

            CoreCurrencyTypeSettings currencySettings = _gameData.Get<CoreCurrencyTypeSettings>(context.user);

            await _newPlayerService.UpdatePlayerOnLogin(context, true);

            await _periodicUpdateService.CheckHourlyCurrencyUpdate(context);
        }
    }
}

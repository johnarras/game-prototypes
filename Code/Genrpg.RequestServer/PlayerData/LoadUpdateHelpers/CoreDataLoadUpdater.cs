
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Entities;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.RequestServer.Trader.NewPlayer;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.NewPlayers.Settings;

namespace Genrpg.RequestServer.PlayerData.LoadUpdateHelpers
{
    public class CoreDataLoadUpdater : IUserLoadUpdater
    {
        private IGameData _gameData = null;
        private IHourlyUpdateService _periodicUpdateService = null;
        private ITraderNewPlayerService _newPlayerService = null;

        public int Order => 1;

        public Type HelperKey => GetType();

        public async Task Update(WebContext context, List<IUnitData> unitData)
        {
            CoreData coreData = context.core;

            NewPlayerBonusSettings newPlayerSettings = _gameData.Get<NewPlayerBonusSettings>(context.core);

            CoreCurrencyTypeSettings currencySettings = _gameData.Get<CoreCurrencyTypeSettings>(context.core);

            await _newPlayerService.UpdatePlayerOnLogin(context, true);

            await _periodicUpdateService.CheckHourlyCurrencyUpdate(context, new HourlyResetArgs() { OnLogin = true });
        }
    }
}



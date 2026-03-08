
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayerData.Services;
using Genrpg.RequestServer.Resets.Entities;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.RequestServer.Trader.NewPlayer;
using Genrpg.RequestServer.Trader.Stats.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.NewPlayers.Settings;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Stats.Services;
using System.Diagnostics.CodeAnalysis;

namespace Genrpg.RequestServer.PlayerData.LoadUpdateHelpers
{
    public class CoreDataLoadUpdater : IUserLoadUpdater
    {
        private IGameData _gameData = null;
        private ITraderNewPlayerService _newPlayerService = null;
        private ILoadPlayerDataService _loadPlayerDataService = null;

        public int Order => 1;

        public Type HelperKey => GetType();

        public async Task Update(WebContext context, List<IUnitData> unitData)
        {
            CoreData coreData = context.core;
            CaravanData caravanData = await context.GetAsync<CaravanData>();    
            TraderStatData statData = await context.GetAsync<TraderStatData>(); 

            await _newPlayerService.UpdatePlayerOnLogin(context, true);

            await _loadPlayerDataService.UpdatePlayerAfterLoginOrLoad(context, true);
        }
    }
}



using OxDb.RequestServer.Core;
using OxDb.RequestServer.PlayerData.Services;
using OxDb.RequestServer.Trader.NewPlayer;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;

namespace OxDb.RequestServer.PlayerData.LoadUpdateHelpers
{
    public class CoreDataLoadUpdater : IUserLoadUpdater
    {
        private ITraderNewPlayerService _newPlayerService = null;
        private ILoadPlayerDataService _loadPlayerDataService = null;

        public EUserLoadUpdateOrder HelperKey => EUserLoadUpdateOrder.Core;

        public async Task Update(WebContext context, List<IUnitData> unitData)
        {
            CoreData coreData = await context.GetAsync<CoreData>();

            await _newPlayerService.UpdatePlayerOnLogin(context, true);

            await _loadPlayerDataService.UpdatePlayerAfterLoginOrLoad(context, true);
        }
    }
}



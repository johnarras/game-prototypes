using OxDb.RequestServer.Core;
using OxDb.RequestServer.PlayerData.Services;
using OxDb.RequestServer.Trader.NewPlayer;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedGame.Attributes.PlayerData;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.Caravans.PlayerData;

namespace OxDb.RequestServer.PlayerData.LoadUpdateHelpers
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
            CoreData coreData = await context.GetAsync<CoreData>();
            CaravanData caravanData = await context.GetAsync<CaravanData>();
            AttributesData attributeData = await context.GetAsync<AttributesData>();

            await _newPlayerService.UpdatePlayerOnLogin(context, true);

            await _loadPlayerDataService.UpdatePlayerAfterLoginOrLoad(context, true);
        }
    }
}




using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayerData.Services;
using Genrpg.RequestServer.Trader.NewPlayer;
using Genrpg.Shared.Attributes.PlayerData;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Trader.Caravans.PlayerData;

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
            CoreData coreData = await context.GetAsync<CoreData>();
            CaravanData caravanData = await context.GetAsync<CaravanData>();
            AttributeData attributeData = await context.GetAsync<AttributeData>();

            await _newPlayerService.UpdatePlayerOnLogin(context, true);

            await _loadPlayerDataService.UpdatePlayerAfterLoginOrLoad(context, true);
        }
    }
}



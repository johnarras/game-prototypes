using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Messages;
using OxDb.ServerCore.Config;
using OxDb.ServerCore.GameSettings.Services;

namespace OxDb.ServerCore.CloudComms.Services
{
    public class BaseAdminService : IAdminService
    {
        protected IServerGameDataService _gameDataService = null;
        protected IServerConfig _config = null;

        virtual public async Task HandleReloadGameState()
        {
            await _gameDataService.ReloadGameData();
        }

        public virtual async Task OnMapUploaded(MapUploadedAdminMessage message)
        {
            await Task.CompletedTask;
        }

        public virtual async Task OnServerStarted(ServerStartedAdminMessage message)
        {
            await Task.CompletedTask;
        }
    }
}



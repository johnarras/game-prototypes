using OxDb.MapServer.Maps;
using OxDb.MapServer.Maps.Services;
using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Messages;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.ServerCore.Constants;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.GameSettings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.MapServer.Admin.Services
{
    public class MapServerAdminService : BaseAdminService, IAdminService
    {
        private IMapServerService _mapServerService = null;
        private IGameData _gameData = null;

        public override async Task HandleReloadGameState()
        {
            await base.HandleReloadGameState();
            IReadOnlyList<MapInstance> instances = _mapServerService.GetMapInstances();

            foreach (MapInstance instance in instances)
            {
                instance.RefreshGameData(_gameData);
            }
        }

        public override async Task OnServerStarted(ServerStartedAdminMessage message)
        {
            if (message.ServerName == ServerNames.InstanceManager)
            {
                _mapServerService.SendAddMapServerMessage();

                IReadOnlyList<MapInstance> mapInstances = _mapServerService.GetMapInstances();

                foreach (MapInstance mapInstance in mapInstances)
                {
                    mapInstance.SendAddInstanceMessage();
                }
            }
            else if (message.ServerName == ServerNames.Player)
            {
                IReadOnlyList<MapInstance> mapInstances = _mapServerService.GetMapInstances();

                foreach (MapInstance mapInstance in mapInstances)
                {
                    mapInstance.SendAllPlayerEnterMapMessages();
                }

            }

            await Task.CompletedTask;
        }

        public override async Task OnMapUploaded(MapUploadedAdminMessage message)
        {

            if (message.WorldDataEnv != _config.DataEnvs[EDataCategories.Worlds.ToString()])
            {
                return;
            }
            await _mapServerService.RestartMapsWithId(message.MapId);


        }
    }
}



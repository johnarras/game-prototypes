using OxDb.MapServer.CloudMessaging.Interfaces;
using OxDb.MapServer.Maps.Services;
using OxDb.MapServer.Setup.MapServer;
using OxDb.ServerCore.Constants;
using OxDb.ServerCore.Core;
using OxDb.ServerCore.MainServer;
using System.Threading.Tasks;

namespace OxDb.MapServer.MainServer
{
    public class MapServerMain : BaseServer<ServerGameState, MapServerSetupService, IMapServerCloudMessageHandler>
    {

        private IMapServerService _mapServerService = null;

        protected override async Task PostInit(ServerInitArgs args)
        {
            await base.PostInit(args);

            InitMapServerData mapData = args.Data as InitMapServerData;

            _instanceId = mapData.MapServerName;
            await _mapServerService.Init(mapData, args.Token);
        }
        protected override bool UseInstanceId => true;
        protected override string GetBaseServerName() { return ServerNames.MapServer; }
    }
}



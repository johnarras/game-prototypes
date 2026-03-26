using Genrpg.MapServer.CloudMessaging.Interfaces;
using Genrpg.MapServer.Maps.Services;
using Genrpg.MapServer.Setup.MapServer;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.MainServer;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.MapServer.MainServer
{
    public class MapServerMain : BaseServer<ServerGameState, MapServerSetupService, IMapServerCloudMessageHandler>
    {

        private IMapServerService _mapServerService = null;

        protected override async Task FinalInit(object data, object parentObject, CancellationToken serverToken)
        {
            await base.FinalInit(data, parentObject, serverToken);

            await _mapServerService.Init(data as InitMapServerData, serverToken);
        }

        protected override string GetServerId(object data)
        {
            InitMapServerData mapData = data as InitMapServerData;
            return CloudServerNames.Map + mapData.MapServerId;
        }
    }
}



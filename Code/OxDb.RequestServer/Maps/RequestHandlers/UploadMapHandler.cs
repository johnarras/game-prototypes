using OxDb.RequestServer.ClientUserRequests.RequestHandlers;
using OxDb.RequestServer.ClientUserRequests.Services;
using OxDb.RequestServer.Core;
using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Messages;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.ServerCore.DataStores.Services;
using OxDb.ServerGame.Maps;
using OxDb.ServerGame.MapSpawns;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedGame.MapServer.WebApi.UploadMap;

namespace OxDb.RequestServer.Maps.RequestHandlers
{
    public class UploadMapHandler : BaseClientUserRequestHandler<UploadMapRequest>
    {
        private IMapDataService _mapDataService = null;
        private IMapSpawnDataService _mapSpawnService = null;
        private ICloudCommsService _cloudCommsService = null;
        private IFullRepositoryService _fullRepoService = null;
        private IGameClientRequestService _gameClientRequestService = null;

        protected override async Task InnerHandleMessage(WebContext context, UploadMapRequest request, CancellationToken token)
        {
            if (EnvNames.IsProdEnv(_config.Env))
            {
                ShowError(context, "Cannot update maps in prod");
                return;
            }

            if (request == null)
            {
                ShowError(context, "No map update data sent");
                return;
            }

            if (request.Map == null)
            {
                ShowError(context, "Missing map on update");
                return;
            }

            if (request.Map.Zones == null || request.Map.Zones.Count < 1)
            {
                ShowError(context, "Map had no zones");
                return;
            }

            if (request.SpawnData == null || request.SpawnData.Data == null)
            {
                ShowError(context, "No spawn data sent to server");
                return;
            }

            await _mapDataService.SaveMap(_fullRepoService, request.Map);

            await _mapSpawnService.SaveMapSpawnData(_fullRepoService, request.SpawnData, request.Map.Id, request.Map.MapVersion);

            await _gameClientRequestService.ResetRequestHandlers();

            MapUploadedAdminMessage mapUploadedMessage = new MapUploadedAdminMessage()
            {
                MapId = request.Map.Id,
                WorldDataEnv = _config.Env,
            };

            _cloudCommsService.SendPubSubMessage(mapUploadedMessage);

            context.AddResponse(new UploadMapResponse());
        }
    }
}



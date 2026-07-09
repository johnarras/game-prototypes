using OxDb.RequestServer.GameClientRequests.Services;
using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Messages;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.SharedCore.DataStores.DataGroups;

namespace OxDb.RequestServer.Admin.Services
{
    public class WebsiteAdminService : BaseAdminService, IAdminService
    {
        IGameClientRequestService _gameClientRequestService = null;
        public override async Task OnMapUploaded(MapUploadedAdminMessage message)
        {
            if (message.WorldDataEnv != _config.DataEnvs[EDataCategories.Worlds.ToString()])
            {
                return;
            }

            await _gameClientRequestService.ResetRequestHandlers();

        }
    }
}



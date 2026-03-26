using Genrpg.RequestServer.Services.WebServer;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.Shared.DataStores.DataGroups;

namespace Genrpg.RequestServer.Services.Admin
{
    public class LoginAdminService : BaseAdminService, IAdminService
    {
        private IWebServerService _loginServerService = null!;
        public override async Task OnMapUploaded(MapUploadedAdminMessage message)
        {
            if (message.WorldDataEnv != _config.DataEnvs[EDataCategories.Worlds.ToString()])
            {
                return;
            }

            await _loginServerService.ResetRequestHandlers();

        }
    }
}



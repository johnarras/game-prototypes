using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Handlers
{
    public class ServerStartedAdminMessageHandler : BaseAdminPubSubMessageHandler<ServerStartedAdminMessage>
    {
        public override Type HelperKey => typeof(ServerStartedAdminMessage);

        protected override async Task InnerHandleMessage(ServerStartedAdminMessage message, CancellationToken token)
        {
            await _adminService.OnServerStarted(message);
        }
    }
}



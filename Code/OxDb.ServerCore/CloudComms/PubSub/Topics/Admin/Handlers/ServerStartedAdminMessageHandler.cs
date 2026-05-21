using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Entities;
using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Messages;

namespace OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Handlers
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



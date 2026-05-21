using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Entities;
using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Messages;

namespace OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Handlers
{
    public class UpdateGameDataAdminMessageHandler : BaseAdminPubSubMessageHandler<UpdateGameDataAdminMessage>
    {
        public override Type HelperKey => typeof(UpdateGameDataAdminMessage);

        protected override async Task InnerHandleMessage(UpdateGameDataAdminMessage message, CancellationToken token)
        {
            _logService.Info("Received Update Game Data Message ");
            await _adminService.HandleReloadGameState();
            await Task.CompletedTask;
        }
    }
}



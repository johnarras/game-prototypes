using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Handlers
{
    public class UpdateGameDataAdminMessageHandler : BaseAdminPubSubMessageHandler<UpdateGameDataAdminMessage>
    {
        public override Type HelperKey => typeof(UpdateGameDataAdminMessage);

        protected override async Task InnerHandleMessage(UpdateGameDataAdminMessage message, CancellationToken token)
        {
            _logService.Message("Received Update Game Data Message ");
            await _adminService.HandleReloadGameState();
            await Task.CompletedTask;
        }
    }
}



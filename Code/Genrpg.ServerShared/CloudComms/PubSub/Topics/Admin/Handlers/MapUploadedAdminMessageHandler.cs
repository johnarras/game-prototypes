using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Handlers
{
    public class MapUploadedAdminMessageHandler : BaseAdminPubSubMessageHandler<MapUploadedAdminMessage>
    {
        public override Type HelperKey => typeof(MapUploadedAdminMessage);

        protected override async Task InnerHandleMessage(MapUploadedAdminMessage message, CancellationToken token)
        {
            await _adminService.OnMapUploaded(message);
        }
    }
}



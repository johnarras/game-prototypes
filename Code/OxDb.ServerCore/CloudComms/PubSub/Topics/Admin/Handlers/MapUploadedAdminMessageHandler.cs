using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Entities;
using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Messages;

namespace OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Handlers
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



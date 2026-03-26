using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages
{
    public class ServerStartedAdminMessage : BaseAdminPubSubMessage
    {
        public string ServerId { get; set; }
    }
}



using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Entities;

namespace OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Messages
{
    public class ServerStartedAdminMessage : BaseAdminPubSubMessage
    {
        public string ServerName { get; set; }
    }
}



using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Entities;

namespace OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Messages
{
    public class MapUploadedAdminMessage : BaseAdminPubSubMessage
    {
        public string MapId { get; set; }
        public string WorldDataEnv { get; set; }
    }
}



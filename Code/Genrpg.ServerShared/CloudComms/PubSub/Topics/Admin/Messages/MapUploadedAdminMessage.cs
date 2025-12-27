using MessagePack;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages
{
    public class MapUploadedAdminMessage : BaseAdminPubSubMessage
    {
        public string MapId { get; set; }
        public string WorldDataEnv { get; set; }
    }
}



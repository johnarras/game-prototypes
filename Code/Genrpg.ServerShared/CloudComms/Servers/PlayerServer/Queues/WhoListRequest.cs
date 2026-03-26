using Genrpg.ServerShared.CloudComms.Queues.Requests.Entities;

namespace Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues
{
    public class WhoListRequest : IPlayerQueueMessage, IRequestQueueMessage
    {
        public string Args { get; set; }
        public string RequestId { get; set; }
        public string FromServerId { get; set; }
    }
}



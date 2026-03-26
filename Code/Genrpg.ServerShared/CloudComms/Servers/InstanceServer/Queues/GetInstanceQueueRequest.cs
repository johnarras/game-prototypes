using Genrpg.ServerShared.CloudComms.Queues.Requests.Entities;

namespace Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues
{
    public class GetInstanceQueueRequest : IInstanceQueueMessage, IRequestQueueMessage
    {
        public string RequestId { get; set; }
        public string FromServerId { get; set; }
        public string MapId { get; set; }
    }
}



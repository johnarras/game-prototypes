using OxDb.ServerCore.CloudComms.Queues.Requests.Entities;

namespace OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues
{
    public class GetInstanceQueueRequest : IInstanceQueueMessage, IRequestQueueMessage
    {
        public string RequestId { get; set; }
        public string FromServerName { get; set; }
        public string MapId { get; set; }
    }
}



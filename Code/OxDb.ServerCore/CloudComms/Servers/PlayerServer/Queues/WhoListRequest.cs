using OxDb.ServerCore.CloudComms.Queues.Requests.Entities;

namespace OxDb.ServerCore.CloudComms.Servers.PlayerServer.Queues
{
    public class WhoListRequest : IPlayerQueueMessage, IRequestQueueMessage
    {
        public string Args { get; set; }
        public string RequestId { get; set; }
        public string FromServerName { get; set; }
    }
}



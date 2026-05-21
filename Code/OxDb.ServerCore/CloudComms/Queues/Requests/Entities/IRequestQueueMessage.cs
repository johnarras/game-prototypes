using OxDb.ServerCore.CloudComms.Queues.Entities;

namespace OxDb.ServerCore.CloudComms.Queues.Requests.Entities
{
    public interface IRequestQueueMessage : IQueueMessage
    {
        public string RequestId { get; set; }
        public string FromServerName { get; set; }
    }
}



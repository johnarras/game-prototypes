using Genrpg.ServerShared.CloudComms.Queues.Entities;

namespace Genrpg.ServerShared.CloudComms.Queues.Requests.Entities
{
    public interface IRequestQueueMessage : IQueueMessage
    {
        public string RequestId { get; set; }
        public string FromServerId { get; set; }
    }
}



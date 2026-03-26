using Genrpg.ServerShared.CloudComms.Queues.Entities;

namespace Genrpg.ServerShared.CloudComms.Queues.Requests.Entities
{
    public interface IResponseQueueMessage : IQueueMessage
    {
        public string RequestId { get; set; }
        public string ErrorText { get; set; }
    }
}



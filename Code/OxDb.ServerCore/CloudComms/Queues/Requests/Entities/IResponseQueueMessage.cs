using OxDb.ServerCore.CloudComms.Queues.Entities;

namespace OxDb.ServerCore.CloudComms.Queues.Requests.Entities
{
    public interface IResponseQueueMessage : IQueueMessage
    {
        public string RequestId { get; set; }
        public string ErrorText { get; set; }
    }
}



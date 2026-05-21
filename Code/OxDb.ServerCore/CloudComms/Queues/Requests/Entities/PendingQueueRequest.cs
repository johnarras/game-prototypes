namespace OxDb.ServerCore.CloudComms.Queues.Requests.Entities
{
    public class PendingQueueRequest
    {
        public string RequestId { get; set; } = null;
        public string ToServerName { get; set; } = null;
        public string FromServerName { get; set; } = null;
        public IRequestQueueMessage Request { get; set; } = null;
        public IResponseQueueMessage Response { get; set; } = null;
        public DateTime SendTime { get; set; } = DateTime.UtcNow;
    }
}



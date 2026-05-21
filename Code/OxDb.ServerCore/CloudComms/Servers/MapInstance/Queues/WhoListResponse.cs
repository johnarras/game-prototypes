using OxDb.ServerCore.CloudComms.Queues.Requests.Entities;

namespace OxDb.ServerCore.CloudComms.Servers.MapInstance.Queues
{

    public class WhoListChar
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ZoneName { get; set; }
        public long Level { get; set; }
    }

    public class WhoListResponse : IMapInstanceQueueMessage, IResponseQueueMessage
    {
        public List<WhoListChar> Chars { get; set; } = new List<WhoListChar>();
        public string RequestId { get; set; }
        public string ErrorText { get; set; }
    }
}



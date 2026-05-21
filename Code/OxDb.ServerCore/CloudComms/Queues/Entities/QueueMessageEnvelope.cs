using Newtonsoft.Json;

namespace OxDb.ServerCore.CloudComms.Queues.Entities
{
    public class QueueMessageEnvelope
    {
        public string ToServerName { get; set; }
        public string FromServerName { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<IQueueMessage> Messages { get; set; } = new List<IQueueMessage>();
    }
}



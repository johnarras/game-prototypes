using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genrpg.ServerShared.CloudComms.Queues.Entities
{
    public class QueueMessageEnvelope
    {
        public string ToServerId { get; set; }
        public string FromServerId { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<IQueueMessage> Messages { get; set; } = new List<IQueueMessage>();
    }
}



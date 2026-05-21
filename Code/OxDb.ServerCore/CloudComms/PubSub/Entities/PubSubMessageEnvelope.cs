using Newtonsoft.Json;

namespace OxDb.ServerCore.CloudComms.PubSub.Entities
{
    public class PubSubMessageEnvelope
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public IPubSubMessage Message { get; set; }
    }
}



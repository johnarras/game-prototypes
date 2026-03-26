using Newtonsoft.Json;

namespace Genrpg.ServerShared.CloudComms.PubSub.Entities
{
    public class PubSubMessageEnvelope
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public IPubSubMessage Message { get; set; }
    }
}



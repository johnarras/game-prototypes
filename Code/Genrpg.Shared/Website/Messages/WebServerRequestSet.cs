using Genrpg.Shared.Website.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genrpg.Shared.Website.Messages
{
    public class WebServerRequestEnvelope
    {
        public string Json { get; set; }
    }

    public class WebServerRequestSet
    {
        public string GameUserId { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<IWebRequest> Requests { get; set; } = new List<IWebRequest>();
    }
}



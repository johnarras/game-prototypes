using Newtonsoft.Json;
using OxDb.SharedCore.Website.Requests.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedCore.Website.Requests.Core
{
    public class WebServerRequestEnvelope
    {
        public string Json { get; set; }
    }

    public class WebServerRequestSet
    {
        public string GameUserId { get; set; }
        public string ClientVersion { get; set; }
        public string ClientPlatform { get; set; }
        public string ClientEnv { get; set; }
        public string SessionId { get; set; }
        public string RequestId { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<IWebRequest> Requests { get; set; } = new List<IWebRequest>();
    }
}



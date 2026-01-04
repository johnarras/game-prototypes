using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genrpg.Shared.Website.Messages
{
    public class WebServerRequestSet
    {
        public string GameUserId { get; set; }
        public string SessionId { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<IWebRequest> Requests { get; set; } = new List<IWebRequest>();
    }
}



using Genrpg.Shared.Website.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genrpg.Shared.Website.Messages
{
    public class WebServerResponseSet
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<IWebResponse> Responses { get; set; } = new List<IWebResponse>();
    }
}



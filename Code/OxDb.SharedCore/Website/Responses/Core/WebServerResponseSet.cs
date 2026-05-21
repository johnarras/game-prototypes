using Newtonsoft.Json;
using OxDb.SharedCore.Website.Responses.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedCore.Website.Responses.Core
{
    public class WebServerResponseSet
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<IWebResponse> Responses { get; set; } = new List<IWebResponse>();
    }
}



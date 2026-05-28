using Newtonsoft.Json;
using OxDb.SharedCore.Website.Requests.Interfaces;
using System.Collections.Generic;
using System.Text;

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
        public string ClientSessionId { get; set; }
        public string RequestId { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<IWebRequest> Requests { get; set; } = new List<IWebRequest>();


        public string ShowRequestNames()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            int numShown = 0;
            foreach (IWebRequest req in Requests)
            {
                if (numShown > 0)
                {
                    sb.Append(",");
                }
                sb.Append(" ");
                sb.Append(req.GetType().Name);
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}



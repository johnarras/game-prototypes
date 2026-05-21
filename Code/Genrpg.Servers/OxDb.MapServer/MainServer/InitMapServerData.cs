using System.Collections.Generic;

namespace OxDb.MapServer.MainServer
{
    public class InitMapServerData
    {
        public int MapServerCount { get; set; }
        public int MapServerIndex { get; set; }
        public string MapServerName { get; set; }
        public List<string> MapIds { get; set; }
        public int StartPort { get; set; }

        public InitMapServerData()
        {
            MapIds = new List<string>();
        }

    }
}



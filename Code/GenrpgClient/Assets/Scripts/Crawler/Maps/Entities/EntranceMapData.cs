using OxDb.SharedGame.Crawler.Maps.Entities;

namespace OxDb.Client.Crawler.Maps.Entities
{
    public class EntranceMapData
    {
        public CrawlerMap EntranceMap { get; set; }
        public int EnterX { get; set; }
        public int EnterZ { get; set; }
        public string EntranceMapName { get; set; }


        public bool IsValid()
        {
            return EntranceMap != null && !string.IsNullOrWhiteSpace(EntranceMapName);
        }
    }
}



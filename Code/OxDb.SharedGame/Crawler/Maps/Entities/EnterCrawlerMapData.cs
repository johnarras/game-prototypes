using OxDb.SharedGame.Crawler.Worlds.Entities;

namespace OxDb.SharedGame.Crawler.Maps.Entities
{
    public class EnterCrawlerMapData
    {
        public long MapId { get; set; }
        public int MapX { get; set; }
        public int MapZ { get; set; }
        public int MapRot { get; set; }

        public CrawlerWorld World { get; set; }
        public CrawlerMap Map { get; set; }
        public bool ReturnToSafety { get; set; }
        public bool IsTownPortal { get; set; }
    }
}



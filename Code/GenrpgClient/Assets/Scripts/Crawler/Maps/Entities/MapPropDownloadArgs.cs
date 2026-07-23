using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.SharedGame.Crawler.Maps.Entities;

namespace OxDb.Client.Crawler.Maps.Entities
{
    public class MapPropDownloadArgs
    {
        public CrawlerMap Map { get; set; }
        public ClientMapCell Cell { get; set; }
        public long X { get; set; }
        public long Z { get; set; }
        public object Data { get; set; }
    }
}



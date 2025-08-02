using Assets.Scripts.Crawler.Maps.GameObjects;
using Genrpg.Shared.Crawler.Maps.Entities;

namespace Assets.Scripts.Crawler.Maps.Entities
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

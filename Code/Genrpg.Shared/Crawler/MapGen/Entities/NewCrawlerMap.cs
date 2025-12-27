using MessagePack;
using Genrpg.Shared.Crawler.Maps.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.MapGen.Entities
{
    public class NewCrawlerMap
    {
        public CrawlerMap Map { get; set; }
        public int EnterX { get; set; }
        public int EnterZ { get; set; }
    }

}



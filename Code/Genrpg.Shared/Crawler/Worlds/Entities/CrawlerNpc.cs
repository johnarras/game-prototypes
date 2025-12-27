using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Worlds.Entities
{
    public class CrawlerNpc : IIdName
    {
        public long IdKey { get; set; }
        public string Name { get; set; }
        public long UnitTypeId { get; set; }
        public long Level { get; set; }
        public long MapId { get; set; }
        public int X { get; set; }
        public int Z { get; set; }
    }
}



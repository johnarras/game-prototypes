using System;
using System.Collections.Generic;
using System.Text;

namespace OxDb.SharedGame.Crawler.Maps.Entities
{
    public class ZoneEdge
    {
        public long ZoneTypeId { get; set; }
        public int X { get; set; }
        public int Z { get; set; }
        public int DX { get; set; }
        public int DZ { get; set; }
    }
}

using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    public class MapPosition
    {
        public long MapId { get; set; }
        public int X { get; set; }
        public int Z { get; set; }
        public int Rot { get; set; }
    }
}



using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    [MessagePackObject]
    public class MapPosition
    {
        [Key(0)] public long MapId { get; set; }
        [Key(1)] public int X { get; set; }
        [Key(2)] public int Z { get; set; }
        [Key(3)] public int Rot { get; set; }
    }
}

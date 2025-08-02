using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Worlds.Entities
{
    [MessagePackObject]
    public class CrawlerNpc : IIdName
    {
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public long UnitTypeId { get; set; }
        [Key(3)] public int Level { get; set; }
        [Key(4)] public long MapId { get; set; }
        [Key(5)] public int X { get; set; }
        [Key(6)] public int Z { get; set; }
    }
}

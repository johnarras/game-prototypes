using MessagePack;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapServer.Entities.MapCache
{
    public class FullCachedMap
    {
        public Map Map { get; set; }
        public CachedMapInstance MapInstance { get; set; }
    }
}



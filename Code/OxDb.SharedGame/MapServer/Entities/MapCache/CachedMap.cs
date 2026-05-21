using System.Collections.Generic;

namespace OxDb.SharedGame.MapServer.Entities.MapCache
{
    public class CachedMap
    {
        public Map FullMap { get; set; }
        public Map ClientMap { get; set; }
        public bool Generating { get; set; }
        public List<CachedMapInstance> Instances { get; set; } = new List<CachedMapInstance>();

    }
}



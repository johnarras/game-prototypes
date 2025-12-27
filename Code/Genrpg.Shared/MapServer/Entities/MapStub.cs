using MessagePack;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.MapServer.Entities
{
    public class MapStub : IStringId, IName
    { 
        public string Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }

        public int BlockCount { get; set; }
        public float ZoneSize { get; set; }

        public MapStub()
        {
            MinLevel = 1;
            MaxLevel = 100;
            BlockCount = 5;
            ZoneSize = 1;
        }

        public void CopyFrom(IMapRoot map)
        {
            Id = map.Id;
            Name = map.Name;
            Desc = map.Desc;
            Icon = map.Icon;
            Art = map.Art;
            MinLevel = map.MinLevel;
            MaxLevel = map.MaxLevel;
            BlockCount = map.BlockCount;
            ZoneSize = map.ZoneSize;
        }
    }
}



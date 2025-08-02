using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapObjects.Entities
{
    
    // MessagePackIgnore
    public class MapObjectGridItem
    {
        public MapObject Obj { get; set; }
        public int GX { get; set; }
        public int GZ { get; set; }
        public int OldGX { get; set; }
        public int OldGZ { get; set; }
    }
}

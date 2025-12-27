using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    public class RiddleHint
    {

        public int Index { get; set; }
        public string Text { get; set; }
    }

    public class MapRiddleHints
    {
        public long RiddleTypeId { get; set; }
        public List<RiddleHint> Hints { get; set; } = new List<RiddleHint>();    
    }

    public class MapEntranceRiddle
    {
        public string Text { get; set; }
        public string Answer { get; set; }
        public string Error { get; set; }
        public long RiddleTypeId { get; set; }
    }
}



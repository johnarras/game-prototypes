using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    [MessagePackObject]
    public class RiddleHint
    {

        [Key(0)] public int Index { get; set; }
        [Key(1)] public string Text { get; set; }
    }

    [MessagePackObject]
    public class MapRiddleHints
    {
        [Key(0)] public long RiddleTypeId { get; set; }
        [Key(1)] public List<RiddleHint> Hints { get; set; } = new List<RiddleHint>();    
    }

    [MessagePackObject]
    public class MapEntranceRiddle
    {
        [Key(0)] public string Text { get; set; }
        [Key(1)] public string Answer { get; set; }
        [Key(2)] public string Error { get; set; }
        [Key(3)] public long RiddleTypeId { get; set; }
    }
}

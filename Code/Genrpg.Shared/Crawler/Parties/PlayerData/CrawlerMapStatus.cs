using Genrpg.Shared.Utils.Data;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    [MessagePackObject]
    public class CrawlerMapStatus
    {
        [Key(0)] public long MapId { get; set; }
        [Key(1)] public int CellsVisited { get; set; }
        [Key(2)] public int TotalCells { get; set; }
        [Key(3)] public long RiddleStatus { get; set; }
        [Key(4)] public SmallIndexBitList Visited { get; set; } = new SmallIndexBitList();
        [Key(5)] public List<PointXZ> OneTimeEncounters { get; set; } = new List<PointXZ>();
    }
}

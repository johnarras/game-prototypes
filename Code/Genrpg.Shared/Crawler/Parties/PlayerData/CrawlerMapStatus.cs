using Genrpg.Shared.Utils.Data;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    public class CrawlerMapStatus
    {
        public long MapId { get; set; }
        public int CellsVisited { get; set; }
        public int TotalCells { get; set; }
        public long RiddleStatus { get; set; }
        public SmallIndexBitList Visited { get; set; } = new SmallIndexBitList();
        public List<PointXZ> OneTimeEncounters { get; set; } = new List<PointXZ>();
    }
}



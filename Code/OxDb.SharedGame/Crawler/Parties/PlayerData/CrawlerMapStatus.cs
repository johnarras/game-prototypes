using OxDb.SharedCore.Utils.Data;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Parties.PlayerData
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



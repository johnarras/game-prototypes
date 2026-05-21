using OxDb.SharedCore.Utils;

namespace OxDb.SharedGame.Zones.Settings
{
    public class ZoneUnitKeyword : IWeightedItem
    {
        public long UnitKeywordId { get; set; }
        public double Weight { get; set; }
    }
}



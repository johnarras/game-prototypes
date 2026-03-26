using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Zones.Settings
{
    public class ZoneUnitKeyword : IWeightedItem
    {
        public long UnitKeywordId { get; set; }
        public double Weight { get; set; }
    }
}



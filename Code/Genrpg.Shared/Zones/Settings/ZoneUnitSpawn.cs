using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Zones.Settings
{
    public class ZoneUnitSpawn : IWeightedItem
    {
        public long UnitTypeId { get; set; }
        public double Weight { get; set; }
    }
}



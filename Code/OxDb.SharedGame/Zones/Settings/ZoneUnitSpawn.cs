using OxDb.SharedCore.Utils;

namespace OxDb.SharedGame.Zones.Settings
{
    public class ZoneUnitSpawn : IWeightedItem
    {
        public long UnitTypeId { get; set; }
        public double Weight { get; set; }
    }
}



using OxDb.SharedCore.Utils;

namespace OxDb.SharedGame.Zones.Settings
{
    public class WeightedObject : IWeightedItem
    {
        public double Weight { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
    }
}

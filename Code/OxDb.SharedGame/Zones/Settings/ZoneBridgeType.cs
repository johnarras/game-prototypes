using OxDb.SharedCore.Utils;

namespace OxDb.SharedGame.Zones.Settings
{
    public class ZoneBridgeType : IWeightedItem
    {
        public long BridgeTypeId { get; set; }
        public string Name { get; set; }
        public double Weight { get; set; }
    }
}



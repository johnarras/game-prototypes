using OxDb.SharedCore.Utils;

namespace OxDb.SharedGame.Zones.Settings
{
    public class ZoneFenceType : IWeightedItem
    {
        public long FenceTypeId { get; set; }
        public double Weight { get; set; }
        public string Name { get; set; }

    }
}



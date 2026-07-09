using OxDb.SharedCore.Utils;

namespace OxDb.SharedGame.Zones.Settings
{
    /// <summary>
    /// Used to override data about trees in the zone and zone type
    /// </summary>
    public class ZoneTreeType : IWeightedItemId
    {
        public long TreeTypeId { get; set; }
        public string Name { get; set; }
        public double Weight { get; set; }

        public long GetId()
        {
            return TreeTypeId;
        }
    }

    public class ZoneBushType : IWeightedItemId
    {
        public long BushTypeId { get; set; }
        public string Name { get; set; }
        public double Weight { get; set; }

        public long GetId()
        {
            return BushTypeId;  
        }
    }
}



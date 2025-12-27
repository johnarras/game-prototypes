using MessagePack;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Zones.Settings
{
    /// <summary>
    /// Used to override data about plant types in the zone type and zone
    /// </summary>
    public class ZoneRockType : IWeightedItem
    {
        public long RockTypeId { get; set; }
        public double Weight { get; set; } = 1.0f;
        public string Name { get; set; }


        public MyColorF BaseColor { get; set; }

        public ZoneRockType()
        {
            BaseColor = new MyColorF();
        }
    }
}



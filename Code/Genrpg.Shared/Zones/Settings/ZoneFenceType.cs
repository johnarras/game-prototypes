using Genrpg.Shared.Utils;
using MessagePack;
namespace Genrpg.Shared.Zones.Settings
{
    public class ZoneFenceType : IWeightedItem
    {
        public long FenceTypeId { get; set; }
        public double Weight { get; set; } = 1;
        public string Name { get; set; }

    }
}



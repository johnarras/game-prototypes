using Genrpg.Shared.Utils;
namespace Genrpg.Shared.Zones.Settings
{
    public class ZoneFenceType : IWeightedItem
    {
        public long FenceTypeId { get; set; }
        public double Weight { get; set; } = 1;
        public string Name { get; set; }

    }
}



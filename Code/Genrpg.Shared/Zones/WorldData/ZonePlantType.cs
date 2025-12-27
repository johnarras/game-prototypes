using MessagePack;
namespace Genrpg.Shared.Zones.WorldData
{
    /// <summary>
    /// Used to override data about plant types in the zone type and zone
    /// </summary>
    public class ZonePlantType
    {
        public long PlantTypeId { get; set; }
        public float Density { get; set; }

        public ZonePlantType()
        {
            Density = 1.0f;
        }
    }
}



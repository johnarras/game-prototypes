using MessagePack;
namespace Genrpg.Shared.Zones.Entities
{
    /// <summary>
    /// Mark a zone as adjacent to this one.
    /// </summary>
    public class ZoneRelation
    {
        public long ZoneId { get; set; }
        public float Offset { get; set; }
    }
}



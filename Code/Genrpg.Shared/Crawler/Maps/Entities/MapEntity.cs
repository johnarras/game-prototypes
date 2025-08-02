using MessagePack;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    [MessagePackObject]
    public class MapEntity
    {
        [Key(0)] public int X { get; set; }
        [Key(1)] public int Z { get; set; }
        [Key(2)] public long EntityTypeId { get; set; }
        [Key(3)] public long EntityId { get; set; }
    }
}

using MessagePack;

namespace Genrpg.Shared.TimedEvents.Entities
{
    [MessagePackObject]
    public class TimedEventCustomReward
    {
        [Key(0)] public int Tier { get; set; }
        [Key(1)] public bool Paid { get; set; }
        [Key(2)] public long EntityTypeId { get; set; }
        [Key(3)] public long EntityId { get; set; }
        [Key(4)] public long Quantity { get; set; }
    }
}

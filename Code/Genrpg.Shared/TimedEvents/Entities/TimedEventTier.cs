using MessagePack;

namespace Genrpg.Shared.TimedEvents.Entities
{
    [MessagePackObject]
    public class TimedEventTier
    {
        [Key(0)] public int Tier { get; set; }
        [Key(1)] public int Points { get; set; }
        [Key(2)] public long FreeEntityTypeId { get; set; }
        [Key(3)] public long FreeEntityId { get; set; }
        [Key(4)] public long FreeQuantity { get; set; }
        [Key(5)] public long PaidEntityTypeId { get; set; }
        [Key(6)] public long PaidEntityId { get; set; }
        [Key(7)] public long PaidQuantity { get; set; }
    }
}

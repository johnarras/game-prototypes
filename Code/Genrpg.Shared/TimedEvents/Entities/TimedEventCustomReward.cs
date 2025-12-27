using MessagePack;

namespace Genrpg.Shared.TimedEvents.Entities
{
    public class TimedEventCustomReward
    {
        public int Tier { get; set; }
        public bool Paid { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public long Quantity { get; set; }
    }
}



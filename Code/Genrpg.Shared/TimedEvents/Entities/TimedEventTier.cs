using MessagePack;

namespace Genrpg.Shared.TimedEvents.Entities
{
    public class TimedEventTier
    {
        public int Tier { get; set; }
        public int Points { get; set; }
        public long FreeEntityTypeId { get; set; }
        public long FreeEntityId { get; set; }
        public long FreeQuantity { get; set; }
        public long PaidEntityTypeId { get; set; }
        public long PaidEntityId { get; set; }
        public long PaidQuantity { get; set; }
    }
}



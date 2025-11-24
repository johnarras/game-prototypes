namespace Assets.Scripts.ClientEvents.Entities
{
    public class AddEntityQuantityVisual
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public long QuantityAdded { get; set; }
        public bool InstantUpdate { get; set; }
    }
}

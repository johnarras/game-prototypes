namespace Assets.Scripts.ClientEvents.Entities
{
    public class SetEntityQuantityVisual
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public long NewQuantity { get; set; }
        public bool InstantUpdate { get; set; }
    }
}



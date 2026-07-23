namespace OxDb.Client.DynamicUI.Interfaces
{
    public interface IEntityQuantityIcon
    {
        public void AddVisualQuantity(long entityTypeId, long entityId, long quantityAdded, bool instant);
    }
}

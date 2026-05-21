using OxDb.SharedGame.Inventory.PlayerData;
namespace OxDb.SharedGame.Inventory.Entities
{
    public class ItemGenArgs
    {
        public Item oldItem { get; set; }
        public long ItemTypeId { get; set; }
        public long QualityTypeId { get; set; }
        public long Quantity { get; set; }
        public long Level { get; set; }
        public long PowerIncrease { get; set; }
        public string CoreNameOverride { get; set; }

        public ItemGenArgs()
        {
            Level = 1;
            QualityTypeId = 0;
            Quantity = 1;
        }
    }
}



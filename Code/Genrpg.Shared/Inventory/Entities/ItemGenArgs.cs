using Genrpg.Shared.Inventory.PlayerData;
using MessagePack;
namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class ItemGenArgs
    {
        [Key(0)] public Item oldItem { get; set; }
        [Key(1)] public long ItemTypeId { get; set; }
        [Key(2)] public long QualityTypeId { get; set; }
        [Key(3)] public long Quantity { get; set; }
        [Key(4)] public int Level { get; set; }
        [Key(5)] public long ExtraItems { get; set; }

        public ItemGenArgs()
        {
            Level = 1;
            QualityTypeId = 0;
            Quantity = 1;
        }
    }
}

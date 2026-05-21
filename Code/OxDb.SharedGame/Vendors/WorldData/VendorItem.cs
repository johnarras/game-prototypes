using MessagePack;
using OxDb.SharedGame.Inventory.PlayerData;
namespace OxDb.SharedGame.Vendors.WorldData
{
    [MessagePackObject]
    public class VendorItem
    {
        [Key(0)] public int Quantity { get; set; }
        [Key(1)] public Item Item { get; set; }
    }
}



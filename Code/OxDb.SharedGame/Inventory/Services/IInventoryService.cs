using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.MapObjects.Entities;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Inventory.Services
{
    public interface IInventoryService : IInjectable
    {
        int InventorySpaceLeft(MapObject obj, Item item);
        ValueTask<bool> AddItem(MapObject obj, Item item, bool forceAdd);
        ValueTask<bool> UnequipItem(MapObject obj, string itemId, bool calcStatsNow = true);
        ValueTask<Item> RemoveItem(MapObject obj, string itemId, bool destroyItem);
        ValueTask<bool> EquipItem(MapObject obj, string itemId, long equipSlotId, bool calcStatsNow = true);
        ValueTask<bool> CanEquipItem(MapObject obj, Item item);
    }
}



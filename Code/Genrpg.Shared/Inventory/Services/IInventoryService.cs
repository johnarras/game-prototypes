using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.MapObjects.Entities;

namespace Genrpg.Shared.Inventory.Services
{
    public interface IInventoryService : IInjectable
    {
        int InventorySpaceLeft(MapObject obj, Item item);
        bool AddItem(MapObject obj, Item item, bool forceAdd);
        bool UnequipItem(MapObject obj, string itemId, bool calcStatsNow = true);
        Item RemoveItem(MapObject obj, string itemId, bool destroyItem);
        bool EquipItem(MapObject obj, string itemId, long equipSlotId, bool calcStatsNow = true);
        bool CanEquipItem(MapObject obj, Item item);
    }
}



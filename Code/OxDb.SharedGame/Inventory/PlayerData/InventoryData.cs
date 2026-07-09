using MessagePack;
using OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Inventory.PlayerData
{

    // This class contains a player's or monster's inventory and equipment and
    // provides an interface into itself since it can get complicated and
    // the inventory vs equipment.

    [MessagePackObject]
    public class InventoryData : OwnerObjectList<Item>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string VersionTag { get; set; }

        public override void SetData(List<Item> data)
        {
            _equipment = data.Where(x => x.EquipSlotId > 0).ToList();
            _inventory = data.Where(x => x.EquipSlotId == 0).ToList();
        }

        public void SetInvenEquip(List<Item> inventory, List<Item> equipment)
        {
            _inventory = inventory;
            _equipment = equipment;
        }

        public override IReadOnlyList<Item> GetData()
        {
            return _inventory.Concat(_equipment).ToList();
        }

        #region Inventory
        protected List<Item> _inventory { get; set; } = new List<Item>();
        public List<Item> GetAllInventory() { return _inventory.ToList(); }
        public void AddInventory(Item item) { _inventory.Add(item); }
        public void RemoveInventory(Item item)
        {
            List<Item> removeItems = _inventory.Where(x => x.Id == item.Id).ToList();

            foreach (Item itemToRemove in removeItems)
            {
                _inventory.Remove(item);
            }
        }
        public virtual Item GetItem(string itemId) { return _inventory.FirstOrDefault(x => x.Id == itemId); }

        public List<Item> GetAllItemsOfItemType(int itemTypeId)
        {
            return _inventory.Where(x => x.ItemTypeId == itemTypeId).ToList() ?? new List<Item>();
        }

        public List<Item> GetItemsByItemTypeId(long itemTypeId)
        {
            return _inventory.Where(x => x.ItemTypeId == itemTypeId).ToList();
        }
        #endregion

        #region Equipment
        protected List<Item> _equipment { get; set; } = new List<Item>();
        public void AddEquipment(Item item) { _equipment.Add(item); }

        public void RemoveEquipment(string itemId)
        {
            _equipment = _equipment.Where(x => x.Id != itemId).ToList();
        }

        public Item GetEquipmentById(string itemId)
        {
            return _equipment.FirstOrDefault(x => x.Id == itemId);
        }

        public List<Item> GetAllEquipment()
        {
            return _equipment.ToList();
        }

        public Item GetEquipBySlot(long equipSlot)
        {
            return _equipment.FirstOrDefault(x => x.EquipSlotId == equipSlot);
        }

        public Item GetEquipById(string itemId)
        {
            return _equipment.FirstOrDefault(x => x.Id == itemId);
        }
        #endregion
    }

    [MessagePackObject]
    public class InventoryDto : OwnerDtoList<InventoryData, Item>
    {
        [Key(0)] public List<Item> AllItems { get; set; } = new List<Item>();
        [Key(1)] public override List<Item> Children { get; set; }
        [Key(2)] public override InventoryData Parent { get; set; }
        [Key(3)] public override string Id { get; set; }
    }
    public class InventoryDataLoader : OwnerDataLoader<InventoryData, Item> { }

    public class InventoryDataMapper : OwnerDataMapper<InventoryData, Item, InventoryDto> { }
}



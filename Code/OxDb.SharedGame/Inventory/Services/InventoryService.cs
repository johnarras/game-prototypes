using OxDb.SharedCore.DataStores.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Inventory.Constants;
using OxDb.SharedGame.Inventory.Messages;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Inventory.Settings.ItemTypes;
using OxDb.SharedGame.Inventory.Settings.Slots;
using OxDb.SharedGame.MapMessages.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Inventory.Services
{
    public class InventoryService : IInventoryService
    {
        private IStatService _statService = null!;
        protected IGameData _gameData = null!;

        public virtual int InventorySpaceLeft(MapObject unit, Item item)
        {
            return 1000;
        }

        public int InventorySpaceLeft(Character ch, Item item)
        {
            return 1000;
        }

        protected virtual void AddMessage(MapObject unit, InventoryData idata, Item item, IMapApiMessage message, EDataUpdateTypes updateType = EDataUpdateTypes.Save)
        {
        }

        protected virtual void AddMessageNear(MapObject unit, InventoryData idata, Item item, IMapApiMessage message, EDataUpdateTypes updateType = EDataUpdateTypes.Save)
        {

        }

        public virtual async ValueTask<bool> AddItem(MapObject unit, Item item, bool forceAdd)
        {
            InventoryData idata = unit.Get<InventoryData>();
            if (idata == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(item.Id))
            {
                item.Id = HashUtils.NewGuid();
            }
            ItemType itype = _gameData.Get<ItemTypeSettings>(unit).Get(item.ItemTypeId);
            if (itype == null)
            {
                return false;
            }

            Item currItem = idata.GetItem(item.Id);
            if (currItem != null)
            {
                return false;
            }
            idata.AddInventory(item);
            item.OwnerId = unit.Id;

            AddMessage(unit, idata, item, new OnAddItem() { ItemId = item.Id, UnitId = unit.Id });
            await Task.CompletedTask;
            return true;
        }

        public virtual async ValueTask<Item> RemoveItem(MapObject unit, string itemId, bool deleteItem)
        {
            InventoryData idata = unit.Get<InventoryData>();

            Item item = idata.GetItem(itemId);
            if (item == null)
            {
                return null!;
            }

            if (idata.GetItem(item.Id) == null)
            {
                return null!;
            }

            idata.RemoveInventory(item);
            AddMessage(unit, idata, item, new OnRemoveItem() { ItemId = item.Id, UnitId = unit.Id },
                deleteItem ? EDataUpdateTypes.Delete : EDataUpdateTypes.Save);
            await Task.CompletedTask;
            return item;
        }

        public virtual async ValueTask<bool> EquipItem(MapObject obj, string itemId, long equipSlotId, bool calcStatsNow = true)
        {

            EquipSlot eqslot = _gameData.Get<EquipSlotSettings>(obj).Get(equipSlotId);
            if (eqslot == null || !eqslot.Active)
            {
                return false;
            }
            InventoryData idata = obj.Get<InventoryData>();

            long oldEquipSlot = -1;

            Item item = idata.GetItem(itemId);
            if (item == null)
            {
                item = idata.GetEquipById(itemId);
                if (item != null)
                {
                    oldEquipSlot = item.EquipSlotId;
                    await UnequipItem(obj, itemId, false);
                }
                else
                {
                    return false;
                }
            }

            if (!await CanEquipItem(obj, item))
            {
                return false;
            }

            ItemType itype = _gameData.Get<ItemTypeSettings>(obj).Get(item.ItemTypeId);

            if (itype == null || itype.EquipSlotId < 1)
            {
                return false;
            }

            List<long> compatibleSlots = itype.GetCompatibleEquipSlots(_gameData, obj);

            if (!compatibleSlots.Contains(equipSlotId))
            {
                return false;
            }


            // Get equipment out of the way.
            Item currEquip = idata.GetEquipBySlot(equipSlotId);
            if (currEquip != null)
            {
                if (itype.HasFlag(ItemFlags.FlagTwoHandedItem) || oldEquipSlot < 1)
                {
                    await UnequipItem(obj, currEquip.Id, false);
                }
                else
                {
                    ItemType currItemType = _gameData.Get<ItemTypeSettings>(obj).Get(currEquip.ItemTypeId);
                    if (currItemType == null || currItemType.HasFlag(ItemFlags.FlagTwoHandedItem) ||
                        currItemType.EquipSlotId == EquipSlots.OffHand)
                    {
                        await UnequipItem(obj, currEquip.Id, false);
                    }
                    else
                    {
                        List<long> currSlots = currItemType.GetCompatibleEquipSlots(_gameData, obj);
                        if (currSlots.Contains(oldEquipSlot))
                        {
                            currEquip.EquipSlotId = oldEquipSlot;
                        }
                    }
                }
            }

            // Remove from inventory.
            await RemoveItem(obj, itemId, false);

            // Two handed weapons remove offhand items.
            if (FlagUtils.MatchesAnyBits(itype.Flags, ItemFlags.FlagTwoHandedItem))
            {
                Item offhandEquip = idata.GetEquipBySlot(EquipSlots.OffHand);
                if (offhandEquip != null)
                {
                    await UnequipItem(obj, offhandEquip.Id, false);
                }
            }

            if (equipSlotId == EquipSlots.OffHand)
            {
                Item mainHandEquip = idata.GetEquipBySlot(EquipSlots.MainHand);
                if (mainHandEquip != null)
                {
                    ItemType mainHandItemType = _gameData.Get<ItemTypeSettings>(obj).Get(mainHandEquip.ItemTypeId);
                    if (mainHandItemType != null && FlagUtils.MatchesAnyBits(mainHandItemType.Flags, ItemFlags.FlagTwoHandedItem))
                    {
                        await UnequipItem(obj, mainHandEquip.Id, false);
                    }
                }
            }

            item.EquipSlotId = equipSlotId;
            idata.AddEquipment(item);
            AddMessageNear(obj, idata, item, new OnEquipItem() { Item = item, UnitId = obj.Id });

            if (calcStatsNow && obj is Unit unit)
            {
                _statService.CalcStats(unit, false);
            }
            await Task.CompletedTask;
            return true;
        }

        public virtual async ValueTask<bool> UnequipItem(MapObject obj, string itemId, bool calcStatsNow = true)
        {
            InventoryData idata = obj.Get<InventoryData>();
            Item item = idata.GetEquipById(itemId);

            if (item == null)
            {
                return false;
            }

            Item currItem = idata.GetEquipmentById(item.Id);

            if (currItem != null)
            {
                idata.RemoveEquipment(currItem.Id);
                AddMessageNear(obj, idata, item, new OnUnequipItem() { ItemId = item.Id, UnitId = obj.Id });
                currItem.EquipSlotId = EquipSlots.None;
                if (calcStatsNow && obj is Unit unit)
                {
                    _statService.CalcStats(unit, false);
                }
            }

            await AddItem(obj, item, true);


            return true;
        }

        public virtual List<Item> GetInventoryFromItemTypeId(MapObject unit, int itemTypeId)
        {
            List<Item> retval = new List<Item>();
            InventoryData idata = unit.Get<InventoryData>();
            if (idata == null)
            {
                return retval;
            }

            return idata.GetItemsByItemTypeId(itemTypeId);
        }

        public async ValueTask<bool> CanEquipItem(MapObject unit, Item item)
        {
            ItemType itype = _gameData.Get<ItemTypeSettings>(unit).Get(item.ItemTypeId);

            if (itype.EquipSlotId < 1)
            {
                return false;
            }

            EquipSlot slot = _gameData.Get<EquipSlotSettings>(unit).Get(itype.EquipSlotId);

            if (slot != null)
            {
                if (!slot.Active)
                {
                    return false;
                }
            }

            return true;
        }
    }
}



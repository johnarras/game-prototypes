using OxDb.MapServer.MapMessaging.Interfaces;
using OxDb.MapServer.Trades.Services;
using OxDb.ServerCore.DataStores.Services;
using OxDb.SharedCore.DataStores.Constants;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Inventory.Services;
using OxDb.SharedGame.MapMessages.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;

namespace OxDb.MapServer.Items.Services
{
    public class ServerInventoryService : InventoryService
    {
        protected IFullRepositoryService _repoService = null;
        private IMapMessageService _messageService = null;
        private ITradeService _tradeService = null;

        public override bool AddItem(MapObject obj, Item item, bool forceAdd)
        {
            return base.AddItem(obj, item, forceAdd);
        }

        public override bool UnequipItem(MapObject obj, string itemId, bool calcStatsNow = true)
        {
            return _tradeService.SafeModifyObject(obj, delegate { return base.UnequipItem(obj, itemId, calcStatsNow); }, false);

        }

        public override Item RemoveItem(MapObject obj, string itemId, bool destroyItem)
        {
            return _tradeService.SafeModifyObject(obj, delegate
            {
                return base.RemoveItem(obj, itemId, destroyItem);
            }, null);
        }

        public override bool EquipItem(MapObject unit, string itemId, long equipSlotId, bool calcStatsNow = true)
        {
            return _tradeService.SafeModifyObject(unit, delegate
            {
                return base.EquipItem(unit, itemId, equipSlotId, calcStatsNow);
            }, false);
        }

        protected override void AddMessage(MapObject unit, InventoryData idata, Item item, IMapApiMessage message, EDataUpdateTypes dataUpdateType = EDataUpdateTypes.Save)
        {
            _messageService.SendMessage(unit, message);
            if (dataUpdateType == EDataUpdateTypes.Save)
            {
                _repoService.QueueSave(item);
            }
            else if (dataUpdateType == EDataUpdateTypes.Delete)
            {
                _repoService.QueueDelete(item);
            }
        }

        protected override void AddMessageNear(MapObject unit, InventoryData idata, Item item, IMapApiMessage message, EDataUpdateTypes dataUpdateType = EDataUpdateTypes.Save)
        {
            _messageService.SendMessageNear(unit, message);
            _repoService.QueueSave(item);
            if (dataUpdateType == EDataUpdateTypes.Save)
            {
                _repoService.QueueSave(item);
            }
            else if (dataUpdateType == EDataUpdateTypes.Delete)
            {
                _repoService.QueueDelete(item);
            }
        }
    }
}



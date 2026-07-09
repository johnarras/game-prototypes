using OxDb.MapServer.Maps;
using OxDb.MapServer.Trades.Services;
using OxDb.ServerCore.DataStores.Services;
using OxDb.ServerGame.Achievements;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Achievements.Constants;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Currencies.PlayerData;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.Messages;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Inventory.Services;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Spawns.WorldData;
using OxDb.SharedGame.Vendors.MapObjectAddons;
using OxDb.SharedGame.Vendors.Settings;
using OxDb.SharedGame.Vendors.WorldData;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.MapServer.Vendors.Services
{
    public interface IVendorService : IInjectable
    {
        void UpdateItems(MapObject mapObject);
        void BuyItem(MapObject obj, BuyItem buyItem);
        void SellItem(MapObject obj, SellItem sellItem);
    }


    public class VendorService : IVendorService
    {
        private IInventoryService _inventoryService = null;
        private IRewardService _rewardService = null;
        private IAchievementService _achievementService = null;
        private ITradeService _tradeService = null;
        private IMapObjectManager _objectManager = null!;
        private IMapProvider _mapProvider = null!;

        private IItemGenService _itemGenService = null;
        protected IFullRepositoryService _repoService = null;
        protected ITextSerializer _serializer = null;
        private IGameData _gameData = null!;

        public void UpdateItems(MapObject mapObject)
        {

            VendorAddon addon = mapObject.GetAddon<VendorAddon>();

            if (addon == null)
            {
                return;
            }

            double refreshMinutes = _gameData.Get<VendorSettings>(mapObject).VendorRefreshMinutes;

            if (refreshMinutes <= 0)
            {
                return;
            }

            if (addon.LastRefreshTime >= DateTime.UtcNow.AddMinutes(-refreshMinutes))
            {
                return;
            }

            int currItemCount = RandUtils.IntRange(addon.ItemCount, addon.ItemCount * 2, mapObject.Rand);
            long level = mapObject.Level;
            Zone zone = _mapProvider.GetMap().Get<Zone>(mapObject.ZoneId);

            if (zone != null)
            {
                level = zone.Level;
            }

            List<VendorItem> newItems = new List<VendorItem>();

            for (int i = 0; i < currItemCount; i++)
            {
                ItemGenArgs igd = new ItemGenArgs()
                {
                    Level = level,
                    Quantity = 1,
                };

                Item item = _itemGenService.Generate(mapObject.Rand, igd);

                if (item != null)
                {
                    newItems.Add(new VendorItem() { Item = item, Quantity = 1 });
                }
            }
            lock (mapObject.OnActionLock)
            {
                if (addon.LastRefreshTime >= DateTime.UtcNow.AddMinutes(-refreshMinutes))
                {
                    return;
                }

                addon.Items = newItems;
                addon.LastRefreshTime = DateTime.UtcNow;

                if (mapObject.Spawn is MapSpawn mapSpawn)
                {
                    mapSpawn = _serializer.MakeCopy(mapSpawn);
                    mapSpawn.AddonString = _serializer.SerializeToString(mapSpawn.Addons);
                    mapSpawn.Addons = null;
                    _repoService.QueueSave(mapSpawn);
                }
            }
        }

        public void BuyItem(MapObject obj, BuyItem buyItem)
        {
            _tradeService.SafeModifyObject(obj, delegate { BuyItemInternal(obj, buyItem); });
        }

        private void BuyItemInternal(MapObject obj, BuyItem buyItem)
        {
            if (!_objectManager.GetObject(buyItem.UnitId, out MapObject vendor))
            {
                obj.SendError("Shopkeeper doesn't exist.");
                return;
            }

            if (!(obj is Character ch))
            {
                obj.SendError("Only players can buy items.");
                return;
            }

            CharCurrencyData cdata = ch.Get<CharCurrencyData>();
            InventoryData idata = ch.Get<InventoryData>();

            long playerMoney = cdata.Data[CharCurrencyTypes.Money];
            long itemPrice = 0;

            VendorAddon addon = vendor.GetAddon<VendorAddon>();

            if (addon == null || addon.Items == null)
            {
                obj.SendError("Shopkeeper doesn't exist.");
                return;
            }

            VendorItem vendorItem = addon.Items.FirstOrDefault(x => x.Item != null && x.Item.Id == buyItem.ItemId);

            if (vendorItem == null || vendorItem.Item == null)
            {
                obj.SendError("Shopkeeper doesn't exist.");
                return;
            }

            if (vendorItem.Quantity > 0)
            {
                lock (vendor.OnActionLock)
                {
                    addon.Items.FirstOrDefault(x => x.Item != null && x.Item.Id == buyItem.ItemId);

                    if (vendorItem == null || vendorItem.Item == null)
                    {
                        obj.SendError("Shopkeeper doesn't exist.");
                        return;
                    }

                    itemPrice = vendorItem.Item.BuyCost;

                    if (itemPrice > playerMoney)
                    {
                        obj.SendError("Shopkeeper doesn't exist.");
                        return;
                    }
                    addon.Items.Remove(vendorItem);
                }
            }

            if (vendorItem != null)
            {
                _ = _rewardService.GiveReward(ch, EntityTypes.CharCurrency, CharCurrencyTypes.Money, -itemPrice, RewardSources.BuyItem, null, 0, null);
                _inventoryService.AddItem(ch, vendorItem.Item, true);
                _achievementService.UpdateAchievement(ch, AchievementTypes.ItemsBought, 1);
            }
        }

        public void SellItem(MapObject obj, SellItem sellItem)
        {
            _tradeService.SafeModifyObject(obj, delegate { SellItemInternal(obj, sellItem); });
        }

        private void SellItemInternal(MapObject obj, SellItem sellItem)
        {
            if (!_objectManager.GetObject(sellItem.UnitId, out MapObject mapObject))
            {
                obj.SendError("That vendor doesn't exist.");
                return;
            }

            VendorAddon addon = mapObject.GetAddon<VendorAddon>();


            if (addon == null)
            {
                obj.SendError("This isn't a vendor.");
                return;
            }


            if (!(obj is Character ch))
            {
                obj.SendError("Only players can sell items.");
                return;
            }

            InventoryData idata = ch.Get<InventoryData>();
            CharCurrencyData cdata = ch.Get<CharCurrencyData>();

            Item item = idata.GetItem(sellItem.ItemId);

            if (item == null)
            {
                obj.SendError("You don't have that item.");
                return;
            }

            long money = (long)(item.BuyCost * _gameData.Get<VendorSettings>(obj).SellToVendorPriceMult);

            _inventoryService.RemoveItem(ch, sellItem.ItemId, true);
            _achievementService.UpdateAchievement(ch, AchievementTypes.ItemsSold, 1);
            _ = _rewardService.GiveReward(ch, EntityTypes.CharCurrency, CharCurrencyTypes.Money, money, RewardSources.SellItem, null, 0, null);
            _achievementService.UpdateAchievement(ch, AchievementTypes.VendorMoney, money);
        }
    }
}



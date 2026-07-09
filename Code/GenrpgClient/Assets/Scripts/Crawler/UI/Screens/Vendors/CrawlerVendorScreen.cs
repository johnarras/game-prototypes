using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.FloatingText.ClientEvents;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.Loot.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Upgrades.Constants;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Inventory.Constants;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Inventory.Services;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Vendors.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class CrawlerVendorScreen : ItemIconScreen
{

    protected ICrawlerService _crawlerService = null;
    protected IInventoryService _inventoryService = null;
    protected ILootGenService _lootGenService = null;
    private ICrawlerWorldService _crawlerWorldService = null;
    private IIconService _iconService = null;
    private ICrawlerUpgradeService _upgradeService = null;
    private IPartyService _partyService = null;

    public const string VendorIconName = "VendorItemIcon";

    public InventoryPanel PlayerItems;
    public GameObject VendorItems;


    public GText PartyGoldText;

    PartyData _party;
    PartyMember _member;

    public override Unit GetUnit() { return _member; }

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);

        _party = _crawlerService.GetParty();
        _member = _party.ActiveParty.First();
        InitPanel();
        ShowVendorItems();
    }

    private void InitPanel()
    {

        InventoryData inventoryData = _member.Get<InventoryData>();

        inventoryData.SetInvenEquip(_party.Inventory, _member.Equipment);

        PlayerItems.Init(InventoryGroup.All, this, _member, null, GetToken());
    }


    private async void ShowVendorItems()
    {
        _clientEntityService.DestroyAllChildren(VendorItems);

        if (VendorItems == null)
        {
            return;
        }

        VendorSettings settings = _gameData.Get<VendorSettings>(null);

        if (_party.VendorItems.Count < 1 || (_party.LastVendorRefresh < DateTime.UtcNow.AddMinutes(-settings.VendorRefreshMinutes)))
        {
            _party.VendorItems = new List<Item>();

            _party.LastVendorRefresh = DateTime.UtcNow;


            int quantity = RandUtils.IntRange(4, 10, _gs.Rand);

            long level = await _crawlerWorldService.GetMapLevelAtParty(_party);

            double quality = _upgradeService.GetPartyBonus(_party, PartyUpgrades.VendorQuality);

            quantity += (int)(10 * _upgradeService.GetPartyBonus(_party, PartyUpgrades.VendorQuality));

            for (int i = 0; i < quantity; i++)
            {
                long qualityTypeId = (long)quality;

                double remainder = quality - qualityTypeId;
                if (_gs.Rand.NextDouble() < remainder)
                {
                    qualityTypeId++;
                }
                ItemGenArgs lootGenData = new ItemGenArgs()
                {
                    Level = level,
                    QualityTypeId = qualityTypeId
                };

                _party.VendorItems.Add(_lootGenService.GenerateItem(lootGenData));
            }
        }

        foreach (Item item in _party.VendorItems)
        {
            InitItemIconData idata = new InitItemIconData()
            {
                Data = item,
                Flags = ItemIconFlags.IsVendorItem | ItemIconFlags.ShowTooltipOnRight,
                IconPrefabName = VendorIconName,
                Screen = this,
            };
            _iconService.InitItemIcon(idata, VendorItems, _assetService, GetToken());
        }

        _uiService.SetText(PartyGoldText, StrUtils.PrintCommaValue(_party.Currencies[CoreCurrencyTypes.Coins]));
    }

    // Blank
    public override void OnLeftClickIcon(ItemIcon icon) { }




    // Equip or Unequip item.
    public override void OnRightClickIcon(ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }


        if (icon.HasFlag(ItemIconFlags.IsVendorItem))
        {
            BuyItem(icon);
        }
        else
        {
            SellItem(icon);
        }
    }


    private void BuyItem(ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }

        Item vendorItem = _party.VendorItems.FirstOrDefault(x => x.Id == icon.GetDataItem().Id);
        if (vendorItem == null)
        {
            _dispatcher.Dispatch(new ShowFloatingText("That item isn't for sale!", EFloatingTextArt.Error));
            return;
        }

        if (vendorItem.BuyCost > _party.Currencies[CoreCurrencyTypes.Coins])
        {
            _dispatcher.Dispatch(new ShowFloatingText("You need more gold to buy this!", EFloatingTextArt.Error));
            return;
        }


        _partyService.AddGold(_party, -vendorItem.BuyCost);

        _party.VendorItems.Remove(icon.GetDataItem());
        _inventoryService.AddItem(_member, icon.GetDataItem(), true);
        ShowVendorItems();
        InitPanel();
    }

    private void SellItem(ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }

        Item item = _party.Inventory.FirstOrDefault(x => x.Id == icon.GetDataItem().Id);

        if (item == null)
        {
            _dispatcher.Dispatch(new ShowFloatingText("You don't have that item!", EFloatingTextArt.Error));
            return;
        }

        _partyService.AddGold(_party, item.SellValue);

        _inventoryService.RemoveItem(_member, icon.GetDataItem().Id, false);
        _party.VendorBuyback.Add(item);

        while (_party.VendorBuyback.Count > 10)
        {
            _party.VendorBuyback.RemoveAt(0);
        }
        ShowVendorItems();
        InitPanel();
    }

    protected override void OnStartClose()
    {
        _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, GetToken());
        base.OnStartClose();
    }
}




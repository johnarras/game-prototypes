
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.Crawlers.Services;
using Genrpg.Shared.Crawler.Currencies.Constants;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Upgrades.Constants;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Vendors.Settings;
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
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);

        _party = _crawlerService.GetParty();
        _member = _party.GetActiveParty().First();
        InitPanel();
        ShowVendorItems();

    }

    private void InitPanel()
    {

        InventoryData inventoryData = _member.Get<InventoryData>();

        inventoryData.SetInvenEquip(_party.Inventory, _member.Equipment);

        PlayerItems.Init(InventoryGroup.All, this, _member, null, _token);
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


            int quantity = MathUtils.IntRange(4, 10, _rand);

            int level = await _crawlerWorldService.GetMapLevelAtParty(_party);

            double quality = _upgradeService.GetPartyBonus(_party, PartyUpgrades.VendorQuality);

            quantity += (int)(10 * _upgradeService.GetPartyBonus(_party, PartyUpgrades.VendorQuality));

            for (int i = 0; i < quantity; i++)
            {
                long qualityTypeId = (long)quality;

                double remainder = quality - qualityTypeId;
                if (_rand.NextDouble() < remainder)
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
            _iconService.InitItemIcon(idata, VendorItems, _assetService, _token);
        }

        _uiService.SetText(PartyGoldText, StrUtils.PrintCommaValue(_party.Currencies.Get(CrawlerCurrencyTypes.Gold)));
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

        if (vendorItem.BuyCost > _party.Currencies.Get(CrawlerCurrencyTypes.Gold))
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
        _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, _token);
        base.OnStartClose();
    }
}


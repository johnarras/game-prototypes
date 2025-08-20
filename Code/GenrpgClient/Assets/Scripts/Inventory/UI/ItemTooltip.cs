using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Inventory.Settings.Ranks;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class InitItemTooltipData : InitTooltipData
{
    public Item MainItem;
    public ItemType MainItemType;
    public bool IsVendorItem;
    public Item CompareToItem;
    public string Message;
    public Unit unit;
}

public class ItemTooltipRowData
{
    public string text;
    public bool isCurrent;
    public long change;
    public int starsToShow;
}


public class ItemTooltip : BaseTooltip
{
    protected ISharedItemService _sharedItemService;
    protected IIconService _iconService;
    public const string ItemTooltipRow = "ItemTooltipRow";

    public const int StarBaseAmount = 25;
    public const int StarIncrementAmount = 25;


    public GText Message;
    public GText ItemName;
    public GText BasicInfo;
    public GImage RarityImage;
    public GameObject RowParent;
    public GText MoneyText;

    protected List<ItemTooltipRow> _rows;

    protected InitItemTooltipData _data;

    protected Unit _unit = null;
    public override void Init(InitTooltipData baseData, CancellationToken token)
    {
        base.Init(baseData, token);
        InitItemTooltipData data = baseData as InitItemTooltipData;
        _data = data;
        if (_data == null || _data.MainItem == null)
        {
            OnExit("No item");
            return;
        }
        _unit = data.unit;

        _uiService.SetText(Message, _data.Message);
        _uiService.SetText(ItemName, _sharedItemService.GetName(_gameData, _unit, _data.MainItem));
        _uiService.SetText(BasicInfo, _sharedItemService.GetBasicInfo(_gameData, _unit, _data.MainItem));

        string bgName = _iconService.GetBackingNameFromQuality(_gameData, _data.MainItem.QualityTypeId);

        _assetService.LoadAtlasSpriteInto(AtlasNames.Icons, bgName, RarityImage, token);

        ShowMoney();

        ShowEffects();
    }

    private void ShowEffects()
    {
        _clientEntityService.DestroyAllChildren(RowParent);
        _rows = new List<ItemTooltipRow>();


        List<ItemEffect> otherEffects = new List<ItemEffect>();
        if (_data.CompareToItem != null && _data.CompareToItem.Effects != null)
        {
            otherEffects = _data.CompareToItem.Effects;
        }

        if (_data.MainItem == null)
        {
            return;
        }
        if (_data.MainItemType != null)
        {
            if (_data.MainItemType.EquipSlotId == EquipSlots.MainHand ||
                _data.MainItem.EquipSlotId == EquipSlots.Ranged)
            {
                LootRank lootRank = _gameData.Get<LootRankSettings>(null).Get(_data.MainItem.LootRankId);

                ItemTooltipRowData rowData = new ItemTooltipRowData()
                {
                    text = "Dam: " + _data.MainItemType.MinVal + "-" + _data.MainItemType.MaxVal + (lootRank != null && lootRank.Damage > 0 ? " (+" + lootRank.Damage + ")" : ""),
                    isCurrent = false,
                    change = 0,
                    starsToShow = 0
                };
                ShowTooltipRow(rowData);
            }
        }

        if (_data.MainItem.Effects == null || _data.MainItem.Effects.Count < 1)
        {
            if (_data.MainItemType != null && _data.MainItemType.Effects != null)
            {
                foreach (ItemEffect eff in _data.MainItemType.Effects)
                {
                    if (eff.EntityTypeId == EntityTypes.Stat || eff.EntityTypeId == EntityTypes.StatPct)
                    {
                        StatType stype = _gameData.Get<StatSettings>(_unit).Get(eff.EntityId);
                        if (stype == null)
                        {
                            continue;
                        }

                        int starsToShow = (int)((eff.Quantity - StarBaseAmount) / StarIncrementAmount + 1);

                        ItemTooltipRowData rowData = new ItemTooltipRowData()
                        {
                            text = stype.Name,
                            isCurrent = false,
                            change = 0,
                            starsToShow = starsToShow,
                        };
                        ShowTooltipRow(rowData);

                    }
                }
            }

        }

        foreach (ItemEffect eff in _data.MainItem.Effects)
        {
            string mainText = EntityUtils.PrintData(_gameData, _unit, eff);

            if (string.IsNullOrEmpty(mainText))
            {
                continue;
            }

            long change = (_data.CompareToItem != null ? -eff.Quantity : 0);
            ItemEffect otherEffect = otherEffects.FirstOrDefault(x => x.EntityTypeId == eff.EntityTypeId && x.EntityId == eff.EntityId);

            if (otherEffect != null)
            {
                change = otherEffect.Quantity - eff.Quantity;
            }

            ItemTooltipRowData rowData = new ItemTooltipRowData()
            {
                text = mainText,
                isCurrent = true,
                change = change,
                starsToShow = 0,
            };
            ShowTooltipRow(rowData);
        }

        foreach (ItemEffect eff in otherEffects)
        {
            ItemEffect mainEffect = _data.MainItem.Effects.FirstOrDefault(x => x.EntityTypeId == eff.EntityTypeId &&
            x.EntityId == eff.EntityId);
            if (mainEffect != null)
            {
                continue;
            }

            string mainText = EntityUtils.PrintData(_gameData, _unit, eff);
            long change = eff.Quantity;
            ItemTooltipRowData rowData = new ItemTooltipRowData()
            {
                text = mainText,
                isCurrent = false,
                change = change,
                starsToShow = 0,
            };
            ShowTooltipRow(rowData);
        }
    }

    private void ShowTooltipRow(ItemTooltipRowData data)
    {
        if (data == null)
        {
            return;
        }

        _assetService.LoadAssetInto(RowParent, AssetCategoryNames.UI,
            ItemTooltipRow, OnLoadRow, data, _token, "Items");
    }

    private void OnLoadRow(object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }

        ItemTooltipRow row = go.GetComponent<ItemTooltipRow>();
        ItemTooltipRowData rowData = data as ItemTooltipRowData;
        if (row == null || rowData == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        row.Init(rowData);
        _rows.Add(row);
    }

    public void OnExit(string reason = "")
    {
        entity.SetActive(false);
    }

    public void ShowMoney()
    {
        if (_data.MainItem != null)
        {
            if (!_data.IsVendorItem)
            {
                _uiService.SetText(MoneyText, "Sell: " + StrUtils.PrintCommaValue(_data.MainItem.SellValue));
            }
            else
            {
                _uiService.SetText(MoneyText, "Price:" + StrUtils.PrintCommaValue(_data.MainItem.SellValue));
            }
        }
    }
}
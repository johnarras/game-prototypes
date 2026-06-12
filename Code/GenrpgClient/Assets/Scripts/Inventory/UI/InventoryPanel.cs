using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Inventory.Constants;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Inventory.Settings.ItemTypes;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class InventoryPanel : BaseBehaviour
{

    protected IIconService _iconService = null;
    public GameObject _iconParent;

    private int _category = 0;
    protected ItemIconScreen _screen = null;
    protected string _prefabName = "";
    private CancellationToken _token;
    private Unit _unit;

    private bool _inInit = false;
    public void Init(int categories, ItemIconScreen screen, Unit unit, string prefabName, CancellationToken token)
    {
        _inInit = true;
        _unit = unit;
        _token = token;
        _screen = screen;
        _category = categories;
        _prefabName = prefabName;

        _clientEntityService.DestroyAllChildren(_iconParent);

        InventoryData inventory = _unit.Get<InventoryData>();

        List<Item> inventoryItems = inventory.GetAllInventory();

        List<Item> finalInventory = new List<Item>();

        foreach (Item item in inventoryItems)
        {
            ItemType itype = _gameData.Get<ItemTypeSettings>(_unit).Get(item.ItemTypeId);
            if (itype == null)
            {
                continue;
            }

            if (itype.EquipSlotId > 0)
            {
                if (FlagUtils.MatchesAnyBits(categories, InventoryGroup.Equipment))
                {
                    finalInventory.Add(item);
                }
            }
            else
            {
                if (FlagUtils.MatchesAnyBits(categories, InventoryGroup.Reagents))
                {
                    finalInventory.Add(item);
                }
            }
        }

        foreach (Item item in finalInventory)
        {
            InitIcon(item, token);
        }
        _inInit = false;
        _screen?.OnUpdateChild(this);
    }

    public void InitIcon(Item item, CancellationToken token)
    {
        InitItemIconData idata = new InitItemIconData()
        {
            Data = item,
            Screen = _screen,
            IconPrefabName = _prefabName,
        };
        _iconService.InitItemIcon(idata, _iconParent, _assetService, token);

        if (!_inInit)
        {
            _screen?.OnUpdateChild(this);
        }
    }

    public void RemoveIcon(string itemId)
    {
        List<ItemIcon> allIcons = _clientEntityService.GetComponents<ItemIcon>(_iconParent);

        ItemIcon desiredIcon = allIcons.FirstOrDefault(x => x.GetDataItem() != null &&
        x.GetDataItem().Id == itemId);

        if (desiredIcon != null)
        {
            _clientEntityService.Destroy(desiredIcon.gameObject);
        }
        _screen?.OnUpdateChild(this);
    }

}


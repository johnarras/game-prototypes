using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Utils;
using System.Threading;


public delegate void OnLoadItemIconHandler(InitItemIconData data);

public class ItemIconFlags
{
    public const int IsVendorItem = (1 << 0);
    public const int ShowTooltipNow = (1 << 1);
    public const int ShowTooltipOnRight = (1 << 2);
    public const int NoDrag = (1 << 3);
}

public class InitItemIconData : DragItemInitData<Item, ItemIcon, ItemIconScreen, InitItemIconData>
{
    public long EntityTypeId;
    public long EntityId;
    public long Quantity;
    public long Level;
    public long Quality;
    public ItemType ItemType;

    public ItemIcon CreatedItem;

    public OnLoadItemIconHandler Handler;

    public string IconPrefabName;

    public string SubDirectory = "Items";
};


public class ItemIcon : DragItem<Item, ItemIcon, ItemIconScreen, InitItemIconData>
{

    protected ISharedItemService _sharedItemService = null;
    protected IEntityService _entityService = null;
    protected IIconService _iconService = null;

    public GImage Icon;
    public GText QuantityText;

    public override void Init(InitItemIconData data, CancellationToken token)
    {
        base.Init(data, token);
        if (data == null)
        {
            return;
        }

        data.CreatedItem = this;
        _initData = data;

        string iconName = ItemConstants.BlankIconName;

        long entityTypeId = data.EntityTypeId;
        long entityId = data.EntityId;

        if (_initData.Data != null)
        {
            entityTypeId = EntityTypes.Item;
            entityId = _initData.Data.ItemTypeId;
            iconName = _sharedItemService.GetIcon(_gs.ch, _initData.Data);
        }
        else
        {
            if (entityTypeId == 0)
            {
                entityTypeId = EntityTypes.Item;
            }
        }

        _spriteService.SetEntityIcon(entityTypeId, entityId, Icon, token, iconName);

        if (_initData.Data != null)
        {
            ItemType itype = _gameData.Get<ItemTypeSettings>(_gs.ch).Get(_initData.Data.ItemTypeId);
            _uiService.SetText(QuantityText, "");
        }
        else
        {
            _uiService.SetText(QuantityText, data.Quantity.ToString());
        }

        if (FlagUtils.MatchesAnyBits(_initData.Flags, ItemIconFlags.ShowTooltipNow))
        {
            ShowTooltip();
            _initData.Flags &= ~ItemIconFlags.ShowTooltipNow;
        }

        if (FlagUtils.MatchesAnyBits(_initData.Flags, ItemIconFlags.NoDrag))
        {
            _canDrag = false;
        }

        data.Handler?.Invoke(data);
    }


    public override void ShowTooltip()
    {
        if (_initData == null || _initData.Screen == null || _initData.Screen.ToolTip == null || _initData.Data == null ||
            _initData.Screen.GetDragItem() != null)
        {
            return;
        }

        _clientEntityService.SetActive(_initData.Screen.ToolTip, true);
        FullItemTooltipInitData fullTooltipInitData = new FullItemTooltipInitData()
        {
            unit = _initData.Screen.GetUnit(),
            screen = _initData.Screen,
            item = _initData.Data,
            flags = _initData.Flags,
        };
        _initData.Screen.ToolTip.Init(fullTooltipInitData, _token);
        UpdateTooltipPosition();
    }

    public override void HideTooltip()
    {
        if (_initData == null || _initData.Screen == null || _initData.Screen.ToolTip == null)
        {
            return;
        }
        _clientEntityService.SetActive(_initData.Screen.ToolTip, false);
    }
}



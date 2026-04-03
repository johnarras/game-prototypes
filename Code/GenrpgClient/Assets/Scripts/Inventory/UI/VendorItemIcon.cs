
using Genrpg.Shared.Utils;
using System.Threading;
using UnityEngine.EventSystems;

public class VendorItemIcon : ItemIcon, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
{

    public GText ItemName;
    public GText ItemInfo;
    public GText MoneyText;

    private long _price = 0;

    public long GetPrice()
    {
        return _price;
    }
    private bool isVendorItem = false;

    public override void Init(InitItemIconData data, CancellationToken token)
    {
        base.Init(data, token);
        if (data == null || data.Data == null)
        {
            return;
        }

        _initData = data;

        isVendorItem = (FlagUtils.MatchesAnyBits(_initData.Flags, ItemIconFlags.IsVendorItem));

        InitItemIconData idata = new InitItemIconData()
        {
            Data = data.Data,
            Flags = data.Flags,
            Handler = data.Handler,
            Screen = data.Screen,
        };


        _uiService.SetText(ItemName, _sharedItemService.GetName(_gs.ch, data.Data));
        _uiService.SetText(ItemInfo, _sharedItemService.GetBasicInfo(_gs.ch, data.Data));

        _price = (isVendorItem ? data.Data.BuyCost : data.Data.SellValue);

        _uiService.SetText(MoneyText, "Price: " + StrUtils.PrintCommaValue(_price));
    }

    public override bool CanDrag()
    {
        return false;
    }





}



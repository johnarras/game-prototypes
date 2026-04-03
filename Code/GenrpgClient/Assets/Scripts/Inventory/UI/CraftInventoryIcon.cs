using System.Threading;
using UnityEngine.EventSystems;

public class CraftInventoryIcon : ItemIcon, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public GText InfoText;

    long currQuantity = 0;

    public void AddToQuantity(long amount)
    {
        _uiService.SetText(QuantityText, currQuantity.ToString());
    }

    public long GetQuantity()
    {
        return currQuantity;
    }

    public override void Init(InitItemIconData data, CancellationToken token)
    {
        base.Init(data, token);
        if (data == null || data.Data == null)
        {
            return;
        }

        _initData = data;

        InitItemIconData idata = new InitItemIconData()
        {
            Data = data.Data,
            Flags = data.Flags,
            Handler = data.Handler,
            Screen = data.Screen,
        };

        _uiService.SetText(InfoText, _sharedItemService.GetBasicInfo(_gs.ch, data.Data));
    }
}




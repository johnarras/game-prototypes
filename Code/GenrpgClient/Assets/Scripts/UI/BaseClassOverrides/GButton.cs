
using Assets.Scripts.UI.Tooltips;
using Genrpg.Shared.UI.Interfaces;
using System.Threading;
using UnityEngine.EventSystems;

public class GButton : UnityEngine.UI.Button, IButton, IPointerEnterHandler, IPointerExitHandler
{
    public TextTooltip Tooltip;

    public CancellationToken GetToken()
    {
        return destroyCancellationToken;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        ShowTooltip(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        ShowTooltip(false);
    }

    private void ShowTooltip(bool visible)
    {
        if (Tooltip != null)
        {
            Tooltip.Show(visible);
        }
    }
}
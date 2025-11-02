
using Assets.Scripts.Assets.ObjectPools;
using Assets.Scripts.UI.Tooltips;
using Genrpg.Shared.UI.Interfaces;
using System;
using System.Threading;
using UnityEngine.EventSystems;

public class GButton : UnityEngine.UI.Button, IButton, IPointerEnterHandler, IPointerExitHandler, IDestroyCallback
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

    protected CancellationTokenRegistration _ctRegistration;

    public void SetDestroyCallback(Action action)
    {
        _ctRegistration.Dispose();

        if (action == null)
        {
            return;
        }

        _ctRegistration = destroyCancellationToken.Register(action);
    }

    protected override void OnDestroy()
    {
        _ctRegistration.Dispose();
    }
}
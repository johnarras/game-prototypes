using Assets.Scripts.Doobers.UI;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class DynamicUIScreen : BaseScreen
{
    public GameObject ScreenSpaceAnchor;
    public GameObject WorldSpaceAnchor;
    public Doober DooberPrefab;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await Task.CompletedTask;
    }

}


using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Minimap.Services;
using OxDb.SharedGame.MapServer.Services;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class MapScreen : BaseScreen
{

    public GameObject ArrowParent = null;
    public GRawImage MapImage = null;
    private IPlayerManager _playerManager = null;
    private IMapProvider _mapProvider = null;
    protected IMapGenData _md = null;
    protected IMinimapService _minimapService = null;

    GameObject ArrowObject = null;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        Setup();

        await Task.CompletedTask;
    }

    private void Setup()
    {
        _assetService.LoadAssetInto(ArrowParent, AssetCategoryNames.UI, "PlayerArrow", OnLoadArrow, GetToken(), default(object), Subdirectory);

        _uiService.SetImageTexture(MapImage, _minimapService.GetTexture());
    }

    private void OnLoadArrow(GameObject go, object data, CancellationToken token)
    {
        ArrowObject = go;
        ShowPlayer();
    }

    protected override void ScreenUpdate()
    {
        ShowPlayer();
        base.ScreenUpdate();
    }


    void ShowPlayer()
    {
        GameObject arrow = ArrowObject;
        if (arrow == null)
        {
            return;
        }

        // Show player on map with arrow.
        GameObject player = _playerManager.GetPlayerGameObject();
        if (player == null)
        {
            return;
        }

        Vector3 pos = player.transform.localPosition;

        if (MapImage == null)
        {
            return;
        }

        // Player pct goes from -0.5 to 0.5.
        float xpct = pos.x / _mapProvider.GetMap().GetHwid() - 0.5f;
        float zpct = pos.z / _mapProvider.GetMap().GetHhgt() - 0.5f;

        float rot = player.transform.eulerAngles.y;

        float imageSize = MapImage.rectTransform.sizeDelta.x;

        float sx = xpct * imageSize;
        float sz = zpct * imageSize;

        Vector3 cpos = arrow.transform.localPosition;
        arrow.transform.localPosition = new Vector3(sx, sz, cpos.z);

        arrow.transform.eulerAngles = new Vector3(0, 0, -rot);


    }

    protected override void OnStartClose()
    {
    }
}





using ClientEvents;
using OxDb.Client.Assets.Constants;
using OxDb.Client.ClientEvents.UI;
using OxDb.Client.Minimap.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapServer.Services;
using System.Threading;
using UnityEngine;

public class MinimapUI : BaseBehaviour
{

    public GameObject MainPanel;
    public GRawImage MapImage;
    public GameObject ArrowParent;
    public GameObject ArrowObject;

    private CancellationToken _token;
    private IPlayerManager _playerManager;
    private IMapProvider _mapProvider;
    protected IMapGenData _md;
    private IMinimapService _minimapService = null;

    public void Init(CancellationToken token)
    {
        _token = token;
        AddListener<EnableMinimapEvent>(OnEnableMinimap);
        AddListener<DisableMinimapEvent>(OnDisableMinimap);
        AddListener<SetMinimapTexture>(OnSetMinimapTexture);
        OnDisableMinimap(null);
        AddUpdate(MinimapUpdate, UpdateTypes.Regular);
        if (ArrowParent != null)
        {
            _assetService.LoadAssetInto(ArrowParent, AssetCategoryNames.UI, "PlayerArrow", OnLoadArrow, token, default(object), "Maps");
        }

        ShowMapImage();
    }

    private void ShowMapImage()
    {
        if (_mapTexture == null)
        {
            _mapTexture = _minimapService.GetTexture();
        }

        if (_mapTexture != null && MapImage != null)
        {
            _uiService.SetImageTexture(MapImage, _mapTexture);
            OnEnableMinimap(null);

        }
        else
        {
            OnDisableMinimap(null);
        }
    }


    private Texture2D _mapTexture = null;

    private void OnSetMinimapTexture(SetMinimapTexture setTexture)
    {
        _mapTexture = setTexture.Texture;
        if (_mapTexture != null)
        {
            _mapTexture.wrapMode = TextureWrapMode.Clamp;
        }
    }

    private void OnLoadArrow(GameObject obj, object data, CancellationToken token)
    {
        ArrowObject = obj;
    }

    void MinimapUpdate()
    {
        GameObject player = _playerManager.GetPlayerGameObject();
        if (player == null || MapImage == null || MapImage.texture == null || _md.GeneratingMap)
        {
            return;
        }
        Vector3 pos = player.transform.localPosition;

        // Player pct goes from -0.5 to 0.5.
        float xpct = pos.x / _mapProvider.GetMap().GetHwid();
        float zpct = pos.z / _mapProvider.GetMap().GetHhgt();

        float imageSize = MapImage.rectTransform.sizeDelta.x;


        if (MapImage.texture != null)
        {
            float sizePct = 256.0f / _mapProvider.GetMap().GetHwid();
            sizePct = MathUtil.Clamp(0.02f, sizePct, 0.15f);
            float xminpct = xpct - sizePct / 2;
            float zminpct = zpct - sizePct / 2;
            MapImage.uvRect = new Rect(new Vector2(xminpct, zminpct), new Vector2(sizePct, sizePct));
        }


        float rot = player.transform.eulerAngles.y;

        if (ArrowObject != null)
        {
            ArrowObject.transform.eulerAngles = new Vector3(0, 0, -rot);
        }



    }

    private void OnEnableMinimap(EnableMinimapEvent data)
    {
        _clientEntityService.SetActive(MainPanel, true);
        return;
    }

    private void OnDisableMinimap(DisableMinimapEvent data)
    {
        _clientEntityService.SetActive(MainPanel, false);
        return;
    }



}




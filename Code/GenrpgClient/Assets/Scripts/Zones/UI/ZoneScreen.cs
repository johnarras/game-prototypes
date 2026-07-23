using OxDb.Client.Assets.Constants;
using OxDb.Client.Minimap.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ZoneScreen : BaseScreen
{

    public GText ZoneName;
    public GRawImage MapImage;
    public GameObject ArrowParent;

    protected GameObject ArrowObject;
    private IPlayerManager _playerManager = null;
    private IMapProvider _mapProvider = null;
    private IZoneStateController _zoneStateController = null;
    private IMinimapService _minimapService = null;


    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        Setup();

        await Task.CompletedTask;
    }

    private void Setup()
    {
        _assetService.LoadAssetInto(ArrowParent, AssetCategoryNames.UI, "PlayerArrow", OnLoadArrow, GetToken(), default(object), "Maps");

        _uiService.SetImageTexture(MapImage, _minimapService.GetTexture());
        ShowPlayer();

    }


    private void OnLoadArrow(GameObject go, object data, CancellationToken token)
    {
        ArrowObject = go;
    }

    protected override void ScreenUpdate()
    {
        ShowPlayer();
    }


    private long _lastZoneShown = -1;
    float xminpct = 0;
    float xmaxpct = 0;
    float zminpct = 0;
    float zmaxpct = 0;


    const float ZonePadPercent = 0.05f;

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

        if (MapImage == null || MapImage.texture == null)
        {
            return;
        }

        Texture mapTexture = MapImage.mainTexture;

        int mapSize = mapTexture.width;

        float imageSize = MapImage.rectTransform.sizeDelta.x;

        float minZonePixelSize = imageSize / 2;

        float minPercentSize = 1.0f * minZonePixelSize / mapSize;

        Zone currZone = _mapProvider.GetMap().Get<Zone>(_zoneStateController.GetCurrentZoneShown());

        float oldminx = 0;
        float oldminz = 0;
        float oldmaxx = 0;
        float oldmaxz = 0;

        if (currZone != null)
        {
            float minx = currZone.MinZ; float minz = currZone.MinX; float maxx = currZone.MaxZ; float maxz = currZone.MaxX;

            xminpct = minx * 1.0f / _mapProvider.GetMap().GetHwid();
            xmaxpct = maxx * 1.0f / _mapProvider.GetMap().GetHhgt();
            zminpct = minz * 1.0f / _mapProvider.GetMap().GetHhgt();
            zmaxpct = maxz * 1.0f / _mapProvider.GetMap().GetHhgt();

            oldminx = xminpct;
            oldminz = zminpct;
            oldmaxx = xmaxpct;
            oldmaxz = zmaxpct;

            float numBlocks = _mapProvider.GetMap().GetHwid() / MapConstants.TerrainPatchSize;

            float edgeSize = 0.01f;

            if (numBlocks > 0)
            {
                edgeSize = Math.Min(0.05f, 1.0f / numBlocks);
            }
            edgeSize = 0;

            float xdiff = xmaxpct - xminpct;
            float zdiff = zmaxpct - zminpct;

            float xmid = (xminpct + xmaxpct) / 2;
            float zmid = (zminpct + zmaxpct) / 2;

            float maxDiff = Math.Max(xdiff, zdiff);

            maxDiff *= (1 + ZonePadPercent);

            if (maxDiff < minPercentSize)
            {
                //dmaxDiff = minPercentSize;
            }

            xminpct = xmid - maxDiff / 2;
            xmaxpct = xmid + maxDiff / 2;
            if (xminpct < 0)
            {
                xmaxpct += -xminpct;
                xminpct = 0;
            }

            if (xmaxpct > 1)
            {
                xminpct -= (xmaxpct - 1);
                xmaxpct = 1;
            }
            zminpct = zmid - maxDiff / 2;
            zmaxpct = zmid + maxDiff / 2;
            if (zminpct < 0)
            {
                zmaxpct += -zminpct;
                zminpct = 0;
            }
            if (zmaxpct > 1)
            {
                zminpct -= (zmaxpct - 1);
                zmaxpct = 1;
            }
            xdiff = maxDiff;
            zdiff = maxDiff;

            if (mapTexture != null)
            {
                MapImage.uvRect = new Rect(new Vector2(xminpct, zminpct), new Vector2(xdiff, zdiff));
            }

            _lastZoneShown = currZone.IdKey;

            _uiService.SetText(ZoneName, currZone.Name);
        }
        else if (currZone == null)
        {
            return;
        }




        if (xminpct >= xmaxpct || zminpct >= zmaxpct)
        {
            return;
        }

        // Player pct goes from -0.5 to 0.5.
        float xpctstart = pos.x / _mapProvider.GetMap().GetHwid();
        float zpctstart = pos.z / _mapProvider.GetMap().GetHhgt();

        float newdx = xmaxpct - xminpct;
        float newdz = zmaxpct - zminpct;

        float xpct = MathUtil.Clamp(0, (xpctstart - xminpct) / (xmaxpct - xminpct), 1) - 0.5f;
        float zpct = MathUtil.Clamp(0, (zpctstart - zminpct) / (zmaxpct - zminpct), 1) - 0.5f;

        float rot = player.transform.eulerAngles.y;

        float sx = xpct * imageSize;
        float sz = zpct * imageSize;

        Vector3 cpos = arrow.transform.localPosition;
        arrow.transform.localPosition = new Vector3(sx, sz, cpos.z);

        arrow.transform.eulerAngles = new Vector3(0, 0, -rot);
    }

}




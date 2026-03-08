using Assets.Scripts.Trader.Travel.ClientEvents;
using Assets.Scripts.Trader.UI.TraderMapUI;
using Assets.Scripts.Trader.WorldMap;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Maps.Services;
using Genrpg.Shared.Trader.Travel.Services;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class TraderTerrain : BaseBehaviour
{


    private ICaravanService _caravanService = null;
    private ICameraController _cameraController = null;
    private ITravelService _travelService = null;
    private ITraderMapService _traderMapService = null;

    public Vector3 CameraOffset = new Vector3(-10, 10, -10);
    public GameObject CityAnchor;
    public TextAsset WorldMapColorIndexes;

    private float _texWidth = 1024;
    private float _texHeight = 512;

    private float _mapToDataRatio = 1;
    private Camera _camera = null;

    public GameObject CaravanAnchor;

    public GameObject PatchAnchor;

    private Dictionary<long, TraderTerrainPatch> _patchesByCoordinate = new Dictionary<long, TraderTerrainPatch>();

    private ConcurrentQueue<TraderTerrainPatch> _patchPool = new ConcurrentQueue<TraderTerrainPatch>();

    public TraderMapCityButton Button;

    public TraderTerrainPatch PatchPrefab;

    int _lastCenterX = -1;
    int _lastCenterY = -1;

    public int TileViewRadius = 5;
    public float CameraOrthgraphicSize = 2.5f;

    public override void Init()
    {
        _camera = _cameraController.GetMainCamera();

        _camera.transform.position = CameraOffset;
        _camera.transform.LookAt(Vector3.zero);
        _camera.orthographic = true;
        _camera.orthographicSize = CameraOrthgraphicSize;
        _dispatcher.AddListener<ShowTraderMapPosition>(OnShowTraderMapPosition, GetToken());
        _dispatcher.AddListener<UpdateTraderMapAngle>(OnUpdateTraderMapAngle, GetToken());

        TextAsset terrainAsset = WorldMapColorIndexes;

        if (terrainAsset != null)
        {
            _travelService.SetTerrainMap(terrainAsset.bytes);
            int lenSquared = terrainAsset.bytes.Length / 2;
            int len = (int)(Math.Sqrt(lenSquared));

            _texWidth = 2 * len;
            _texHeight = len;
        }

        SetupMapImage();
    }

    private void SetupMapImage()
    {
        int cdx = 0;
        int cdy = 0;

        IReadOnlyList<City> cities = _gameData.Get<CitySettings>(_gs.ch).GetData();

        CityAnchor.transform.position = new Vector3(cdx, 0.1f, cdy);

        foreach (City city in cities)
        {
            TraderMapCityButton button = _clientEntityService.FullInstantiate(Button);
            _clientEntityService.AddToParent(button, CityAnchor);
            button.transform.localPosition = new Vector3(city.MapPixelX * _mapToDataRatio, 0.2f, (_texHeight - 1) - city.MapPixelY * _mapToDataRatio);
        }


        ShowCurrentMapPosition(true);
    }

    private void ShowCurrentMapPosition(bool fullRefresh)
    {

        CoreData coreData = _gs.ch.Get<CoreData>();

        CaravanPosition pos = _caravanService.GetPosition(coreData);

        MyPointF posPoint = _traderMapService.GetMapCoordinate(pos.FromX, pos.FromY, pos.ToX, pos.ToY, pos.DistanceGone, pos.TotalDistanceToTarget);
        float xpos = posPoint.X;
        float ypos = posPoint.Y;

        ShowPos(xpos, ypos, true, fullRefresh);
    }

    private void OnUpdateTraderMapAngle(UpdateTraderMapAngle angle)
    {
        ShowCaravanAngle();
    }

    private void OnShowTraderMapPosition(ShowTraderMapPosition pos)
    {
        ShowPos(pos.X, pos.Y, pos.UpdateAngle, pos.FullRefresh);
    }

    private void ShowCaravanAngle()
    {
        CaravanPosition pos = _caravanService.GetPosition(_gs.ch.Get<CoreData>());

        CaravanAnchor.transform.eulerAngles = new Vector3(0, pos.Angle, 0);

    }

    private void ShowPos(float x, float y, bool updateAngle, bool fullRefresh)
    {
        x *= _mapToDataRatio;
        y *= _mapToDataRatio;

        Vector3 worldPos = new Vector3(x, 0, _texHeight - y);
        _camera.transform.position = worldPos + CameraOffset;
        _camera.transform.LookAt(worldPos);
        CaravanAnchor.transform.position = worldPos + Vector3.up * 1.0f;

        if (updateAngle)
        {
            ShowCaravanAngle();
        }

        ShowMapAroundCenter(x, y, fullRefresh);
    }

    private long GetIndexFromPos(int x, int y)
    {
        return x * 100000 + y;
    }

    private void ShowMapAroundCenter(float x, float y, bool fullRefresh)
    {
        int cx = (int)x;
        int cy = (int)y;

        if (!fullRefresh && cx == _lastCenterX && cy == _lastCenterY)
        {
            return;
        }

        for (int xx = cx - TileViewRadius; xx <= cx + TileViewRadius; xx++)
        {
            for (int yy = cy - TileViewRadius; yy <= cy + TileViewRadius; yy++)
            {
                float dx = xx - cx;
                float dy = yy - cy;

                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (Mathf.Ceil(dist) <= TileViewRadius)
                {

                    long index = GetIndexFromPos(xx, yy);

                    if (_patchesByCoordinate.ContainsKey(index))
                    {
                        continue;
                    }

                    int biomeIndex = _travelService.GetTerrainIndex(xx, yy);

                    TraderTerrainPatch patch = CheckoutPatch();

                    _clientEntityService.AddToParent(patch, PatchAnchor);
                    patch.ShowTerrain(this, xx, (int)_texHeight - yy - 1, cx, cy, biomeIndex);

                    _patchesByCoordinate[index] = patch;

                }
            }
        }

        List<long> removeCoordinates = new List<long>();

        foreach (long coord in _patchesByCoordinate.Keys)
        {
            TraderTerrainPatch patch = _patchesByCoordinate[coord];
            if (Math.Floor(patch.GetDistanceToPoint(cx, (int)_texHeight - cy - 1)) > TileViewRadius)
            {
                removeCoordinates.Add(coord);
            }
        }

        foreach (long coord in removeCoordinates)
        {
            if (_patchesByCoordinate.TryGetValue(coord, out TraderTerrainPatch patch))
            {
                patch.HideTerrain();
                _patchesByCoordinate.Remove(coord);
            }
        }
    }

    private TraderTerrainPatch CheckoutPatch()
    {
        if (_patchPool.TryDequeue(out TraderTerrainPatch patch))
        {
            _clientEntityService.SetActive(patch, true);
            return patch;
        }

        patch = _clientEntityService.FullInstantiate<TraderTerrainPatch>(PatchPrefab);

        return patch;
    }

    public void ReturnPatch(TraderTerrainPatch patch)
    {
        _patchPool.Enqueue(patch);
        _clientEntityService.SetActive(patch, false);
    }
}




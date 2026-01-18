using Assets.Scripts.Trader.Travel.ClientEvents;
using Assets.Scripts.Trader.UI.TraderMapUI;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Maps.Services;
using Genrpg.Shared.Trader.Travel.Services;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;
using UnityEngine;

public class TraderTerrain : BaseBehaviour
{


    private ICaravanService _caravanService = null;
    private ICameraController _cameraController = null;
    private ITravelService _travelService = null;
    private ITraderMapService _traderMapService = null;

    public Vector3 CameraOffset = new Vector3(0, 20, 0);
    public GameObject InnerTerrain;
    public MeshRenderer Renderer;
    public GameObject CityAnchor;
    public TextAsset WaterMask;

    public float DataMapWidth = 2048;
    public float DataMapHeight = 1024;

    private float _texWidth = 1024;
    private float _texHeight = 512;

    private float _mapToDataRatio = 1;
    private Camera _camera = null;

    public GameObject CaravanAnchor;

    public TraderMapCityButton Button;

    private byte[] _maskData = null;

    public override void Init()
    {
        _camera = _cameraController.GetMainCamera();

        _camera.transform.position = new Vector3(-10, 20, -10);
        _camera.transform.LookAt(Vector3.zero);
        _camera.orthographic = true;
        _camera.orthographicSize = 256;
        _dispatcher.AddListener<ShowTraderMapPosition>(OnShowTraderMapPosition, GetToken());
        _dispatcher.AddListener<UpdateTraderMapAngle>(OnUpdateTraderMapAngle, GetToken());

        _maskData = WaterMask.bytes;

        _travelService.SetWaterMask(_maskData);

        SetupMapImage();
    }

    private void SetupMapImage()
    {
        if (Renderer != null && Renderer.material != null)
        {
            Material mat = Renderer.material;

            if (mat.mainTexture != null)
            {
                _texWidth = mat.mainTexture.width;
                _texHeight = mat.mainTexture.height;

                _mapToDataRatio = _texWidth / DataMapWidth;
                InnerTerrain.transform.localScale = new Vector3(_texWidth, _texHeight, 1);
            }
        }

        int cdx = 0;
        int cdy = 0;

        IReadOnlyList<City> cities = _gameData.Get<CitySettings>(_gs.ch).GetData();

        CityAnchor.transform.position = new Vector3(-_texWidth / 2 + cdx, 0.5f, -_texHeight / 2 + cdy);

        foreach (City city in cities)
        {
            TraderMapCityButton button = _clientEntityService.FullInstantiate(Button);
            _clientEntityService.AddToParent(button, CityAnchor);
            button.transform.localPosition = new Vector3(city.MapPixelX * _mapToDataRatio, 0, _texHeight - city.MapPixelY * _mapToDataRatio);
        }


        ShowCurrentMapPosition();
    }

    private void ShowCurrentMapPosition()
    {

        CoreData coreData = _gs.ch.Get<CoreData>();

        CaravanPosition pos = _caravanService.GetPosition(coreData);

        MyPointF posPoint = _traderMapService.GetMapCoordinate(pos.FromX, pos.FromY, pos.ToX, pos.ToY, pos.DistanceGone, pos.DistanceToTarget);
        float xpos = posPoint.X;
        float ypos = posPoint.Y;

        ShowPos(xpos, ypos, true);
    }

    private void OnUpdateTraderMapAngle(UpdateTraderMapAngle angle)
    {
        ShowCaravanAngle();
    }

    private void OnShowTraderMapPosition(ShowTraderMapPosition pos)
    {
        ShowPos(pos.X, pos.Y, pos.UpdateAngle);
    }

    private void ShowCaravanAngle()
    {
        CaravanPosition pos = _caravanService.GetPosition(_gs.ch.Get<CoreData>());

        CaravanAnchor.transform.eulerAngles = new Vector3(0, pos.Angle, 0);

    }

    private void ShowPos(float x, float y, bool updateAngle)
    {
        x *= _mapToDataRatio;
        y *= _mapToDataRatio;

        x -= _texWidth / 2;
        y = _texHeight / 2 - y;
        Vector3 pos = new Vector3(x, 0, y);
        _camera.transform.position = pos + CameraOffset;
        _camera.transform.LookAt(pos);
        CaravanAnchor.transform.position = pos;

        if (updateAngle)
        {
            ShowCaravanAngle();
        }
    }
}




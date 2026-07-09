
using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Assets.ObjectPools;
using Assets.Scripts.Awaitables;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Crawler.Maps.Props;
using Assets.Scripts.MapTerrain;
using Assets.Scripts.Trader.Travel.ClientEvents;
using Assets.Scripts.Trader.WorldMap.Constants;
using Assets.Scripts.Trader.WorldMap.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.ProcGen.Settings.Props;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Maps.Services;
using OxDb.SharedGame.Trader.Maps.Settings;
using OxDb.SharedGame.Trader.Travel.Services;
using OxDb.SharedGame.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class PropPosition
{
    public TraderTerrainPatch Patch { get; set; }
    public Vector3 Pos { get; set; }
    public Vector3 Scale { get; set; }
    public Vector3 EulerAngles { get; set; }
    public int X { get; set; }
    public int Z { get; set; }
}

public class TraderTerrain : BaseBehaviour, IClientResetCleanup
{

    private ICaravanService _caravanService = null;
    private ICameraController _cameraController = null;
    private ITravelService _travelService = null;
    private ITraderMapService _traderMapService = null;
    private IAwaitableService _awaitableService = null;
    private ITerrainTextureManager _textureManager = null;
    private IMapTerrainManager _mapTerrainManager = null;


    public float CameraYOffset = 5;
    public float CameraXOffset = 0;
    public float CameraZOffset = -5;
    public TextAsset WorldMapColorIndexes;
    public TraderTerrainPatch PatchPrefab;

    private Camera _camera = null;

    public float PropChanceScale = 0.33f;

    public GameObject OverallAnchor;
    public GameObject CaravanAnchor;
    public GameObject PropAnchor;


    public GameObject PatchAnchor;

    public float DataToImageSizeRatio = 4;

    private Queue<TraderTerrainPatch> _patchCache = new Queue<TraderTerrainPatch>();


    private IObjectPool _propPool = null;

    private IndexedTerrainLayer[] _indexedLayerCache = null;

    private TerrainLayer[] _terrainLayerCache = null;

    private float[,] _heightNoise = null;

    private byte[,] _zoneTypeIds = null;
    private byte[,] _colorIndexes = null;

    private float[] _zoneTypePropChances = null;

    private List<WeightedObject>[] _zoneTypeProps = null;

    private Dictionary<long, long> _textureTypeToZoneTypeDict = new Dictionary<long, long>();

    float _heightNoiseScale = 0.01f;
    private long _roadTextureTypeId = 0;

    private List<TraderTerrainPatch> _currPatches = new List<TraderTerrainPatch>();

    private List<CrawlerProp> _props = new List<CrawlerProp>();

    public float PropVisibilityRadius = 2;

    private int _mapWidth = 0;
    private int _mapHeight = 0;

    public override void Init()
    {
        _camera = _cameraController.GetMainCamera();

        _camera.transform.LookAt(Vector3.zero);
        _dispatcher.AddListener<ShowTraderMapPosition>(OnShowTraderMapPosition, GetToken());

        TextAsset terrainAsset = WorldMapColorIndexes;

        ZoneTypeSettings zoneTypeSettings = _gameData.Get<ZoneTypeSettings>(_gs.ch);

        long maxZoneTypeId = zoneTypeSettings.GetData().Max(x => x.IdKey);

        _zoneTypePropChances = new float[maxZoneTypeId + 1];
        _zoneTypeProps = new List<WeightedObject>[maxZoneTypeId + 1];


        foreach (ZoneType zoneType in zoneTypeSettings.GetData())
        {
            if (zoneType.PropChance > 0 && zoneType.Props.Count > 0)
            {
                _zoneTypePropChances[zoneType.IdKey] = (float)zoneType.PropChance * PropChanceScale;
                _zoneTypeProps[zoneType.IdKey] = zoneType.Props;
            }
        }


        if (terrainAsset != null)
        {
            _travelService.SetTerrainMap(terrainAsset.bytes);
            int lenSquared = terrainAsset.bytes.Length / 2;
            int len = (int)(Math.Sqrt(lenSquared));

            _mapHeight = len;
            _mapWidth = len * 2;

            float freq = len * 0.22f;

            _heightNoise = new float[len * 2, len];

            _zoneTypeIds = new byte[len * 2, len];

            _colorIndexes = new byte[len * 2, len];

            IndexedColorSettings colorSettings = _gameData.Get<IndexedColorSettings>(_gs.ch);

            foreach (ZoneType zoneType in zoneTypeSettings.GetData())
            {
                _textureTypeToZoneTypeDict[zoneType.BaseTextureTypeId] = zoneType.IdKey;
            }


            for (int x = 0; x < len * 2; x++)
            {
                for (int z = 0; z < len; z++)
                {
                    int biomeIndex = _travelService.GetTerrainIndex(x, z);

                    long textureTypeId = (byte)colorSettings.Get(biomeIndex).TextureTypeId;

                    if (_textureTypeToZoneTypeDict.TryGetValue(textureTypeId, out long zoneTypeId))
                    {
                        _zoneTypeIds[x, z] = (byte)zoneTypeId;
                    }

                    _colorIndexes[x, z] = (byte)biomeIndex;
                }
            }
        }

        _roadTextureTypeId = _gameData.Get<IndexedColorSettings>(_gs.ch).RoadTextureTypeId;
        _awaitableService.ForgetAwaitable(LoadTerrainTextures());

        PatchAnchor.transform.localPosition = new Vector3(-TraderTerrainConstants.PatchRadius, 0, -TraderTerrainConstants.PatchRadius);
    }

    public async Task OnReset(CancellationToken token)
    {
        _clientEntityService.DestroyAllChildren(PatchAnchor);
        _patchCache = new Queue<TraderTerrainPatch>();
        _currPatches = new List<TraderTerrainPatch>();

        await Task.CompletedTask;
    }

    private async Awaitable LoadTerrainTextures()
    {

        IReadOnlyList<IndexedColor> indexedColors = _gameData.Get<IndexedColorSettings>(_gs.ch).GetData();

        _indexedLayerCache = new IndexedTerrainLayer[indexedColors.Count];
        _terrainLayerCache = new TerrainLayer[indexedColors.Count];


        for (int i = 0; i < indexedColors.Count; i++)
        {
            _indexedLayerCache[i] = new IndexedTerrainLayer() { Index = (int)indexedColors[i].IdKey, TextureTypeId = indexedColors[i].TextureTypeId };
            _textureManager.SetupTerrainTexture(_indexedLayerCache[i], GetToken());
        }

        try
        {
            while (_indexedLayerCache.Any(x => x.TerrainLayer == null))
            {
                await Awaitable.NextFrameAsync(GetToken());
            }
        }
        catch (Exception ee)
        {
            _logService.Exception(ee, "TraderTerrainInit");
        }

        for (int i = 0; i < _indexedLayerCache.Count(); i++)
        {
            _terrainLayerCache[i] = _indexedLayerCache[i].TerrainLayer;
        }

        await ShowMapImage();
    }


    private async ValueTask ShowMapImage()
    {
        CoreData coreData = _gs.ch.Get<CoreData>();

        CaravanPosition pos = await _caravanService.GetPosition(_gs.ch);

        ShowTraderMapPosition showPos = new ShowTraderMapPosition() { Pos = pos, FullRefresh = true };

        OnShowTraderMapPosition(showPos);
    }


    private void RemovePatch(TraderTerrainPatch patch)
    {
        if (_currPatches.Contains(patch))
        {
            _currPatches.Remove(patch);
        }

        _patchCache.Enqueue(patch);
        _clientEntityService.SetActive(patch, false);

    }

    float _lastUpdateMapX = -1;
    float _lastUpdateMapZ = -1;

    int _patchLoadingCount = 0;
    private void OnShowTraderMapPosition(ShowTraderMapPosition showPos)
    {
        _ = OnShowTraderMapPositionAsync(showPos);
    }

    private async ValueTask OnShowTraderMapPositionAsync(ShowTraderMapPosition showPos)
    {
        CaravanPosition pos = showPos.Pos;

        if (pos == null)
        {
            pos = await _caravanService.GetPosition(_gs.ch);
        }

        double distanceGone = showPos.DistanceGone;

        if (distanceGone < 0)
        {
            distanceGone = pos.DistanceGone;
        }
        Point2F mapCoord = _traderMapService.GetMapCoordinate(pos.FromX, pos.FromZ, pos.ToX, pos.ToZ, distanceGone, pos.TotalDistanceToTarget);


        mapCoord.Z = 4096 - mapCoord.Z - 1;
        mapCoord.X /= DataToImageSizeRatio;
        mapCoord.Z /= DataToImageSizeRatio;

        CaravanAnchor.transform.localPosition = new Vector3(mapCoord.X, 0, mapCoord.Z) + Vector3.up * 0.05f;
        Vector3 caravanPos = CaravanAnchor.transform.position;
        CaravanAnchor.transform.eulerAngles = new Vector3(0, pos.Angle, 0);
        float angle = pos.Angle;
        angle = 0;
        float xOffset = Mathf.Cos(angle * Mathf.PI / 180);
        float zOffset = Mathf.Sin(angle * Mathf.PI / 180);
        _camera.transform.position = caravanPos + new Vector3(CameraXOffset, CameraYOffset, CameraZOffset);
        _camera.transform.LookAt(caravanPos);

        if (Math.Abs(mapCoord.X - _lastUpdateMapX) < TraderTerrainConstants.DistanceBeforePatchCheck &&
            Math.Abs(mapCoord.Z - _lastUpdateMapZ) < TraderTerrainConstants.DistanceBeforePatchCheck)
        {
            if (_gs.Rand.NextDouble() < 0.75f)
            {
                UpdateProps((int)mapCoord.X, (int)mapCoord.Z);
            }
            return;
        }

        _lastUpdateMapX = mapCoord.X;
        _lastUpdateMapZ = mapCoord.Z;

        UpdateProps((int)mapCoord.X, (int)mapCoord.Z);

        int cx = (int)(mapCoord.X + TraderTerrainConstants.PatchRadius) / TraderTerrainConstants.PatchWorldWidth;

        int cz = (int)(mapCoord.Z + TraderTerrainConstants.PatchRadius) / TraderTerrainConstants.PatchWorldWidth;

        List<Point2I> patchesToAdd = new List<Point2I>();

        int viewRadius = TraderTerrainConstants.ViewRadius;

        for (int x = cx - viewRadius; x <= cx + viewRadius; x++)
        {
            int worldX = x * TraderTerrainConstants.PatchWorldWidth;

            for (int z = cz - viewRadius; z <= cz + viewRadius; z++)
            {
                int worldZ = z * TraderTerrainConstants.PatchWorldWidth;

                if (!_currPatches.Any(p => p.XPos == worldX && p.ZPos == worldZ))
                {
                    patchesToAdd.Add(new Point2I() { X = worldX, Z = worldZ });
                }
            }
        }


        List<TraderTerrainPatch> patchesToRemove = new List<TraderTerrainPatch>();
        foreach (TraderTerrainPatch patch in _currPatches)
        {
            if (patch.XPos / TraderTerrainConstants.PatchWorldWidth < cx - viewRadius || patch.ZPos / TraderTerrainConstants.PatchWorldWidth < cz - viewRadius ||
                patch.XPos / TraderTerrainConstants.PatchWorldWidth > cx + viewRadius || patch.ZPos / TraderTerrainConstants.PatchWorldWidth > cz + viewRadius)
            {
                patchesToRemove.Add(patch);
            }
        }

        foreach (TraderTerrainPatch patch in patchesToRemove)
        {
            RemovePatch(patch);
        }

        foreach (Point2I pt in patchesToAdd)
        {
            if (!showPos.FullRefresh && _patchLoadingCount > 0)
            {
                break;
            }

            _awaitableService.ForgetAwaitable(ShowPatchAsync(pt.X, pt.Z));

            if (!showPos.FullRefresh)
            {
                break;
            }
        }
    }

    private async Awaitable ShowPatchAsync(int patchXPos, int patchZPos)
    {


        try
        {

            _patchLoadingCount++;
            ZoneTypeSettings zoneTypeSettings = _gameData.Get<ZoneTypeSettings>(_gs.ch);
            if (_currPatches.Any(p => p.XPos == patchXPos && p.ZPos == patchZPos))
            {
                _patchLoadingCount--;
                return;
            }

            IndexedColorSettings colorSettings = _gameData.Get<IndexedColorSettings>(_gs.ch);

            TraderTerrainPatch patch = null;

            if (!_patchCache.TryDequeue(out patch))
            {
                patch = _clientEntityService.FullInstantiate<TraderTerrainPatch>(PatchPrefab);
                patch.Core.TerrainSize = TraderTerrainConstants.PatchSize;
                await _mapTerrainManager.InitTerrainContainer(patch, GetToken());
                await Awaitable.NextFrameAsync(GetToken());

                _clientEntityService.AddToParent(patch.Core.Terrain.gameObject, patch.TerrainParent);
                patch.Core.Terrain.gameObject.transform.localPosition = Vector3.zero;
                patch.Heights = new float[TraderTerrainConstants.PatchSize, TraderTerrainConstants.PatchSize];
                patch.Alphas = new float[TraderTerrainConstants.PatchSize, TraderTerrainConstants.PatchSize, TraderTerrainConstants.AlphaMapCount];
                await Awaitable.NextFrameAsync(GetToken());
                await Awaitable.NextFrameAsync(GetToken());
                _clientEntityService.AddToParent(patch, PatchAnchor);
                patch.Core.TerrainData.terrainLayers = _terrainLayerCache;
            }

            await Awaitable.NextFrameAsync(GetToken());
            patch.XPos = patchXPos;
            patch.ZPos = patchZPos;

            patch.name = "Patch" + patchXPos + "." + patchZPos;
            patch.transform.localPosition = new Vector3(patchXPos, 0, patchZPos);
            _clientEntityService.SetActive(patch.gameObject, true);

            _currPatches.Add(patch);

            for (int x = 0; x < patch.Heights.GetLength(0); x++)
            {
                for (int z = 0; z < patch.Heights.GetLength(1); z++)
                {
                    patch.Heights[x, z] = 0;
                    for (int a = 0; a < patch.Alphas.GetLength(2); a++)
                    {
                        patch.Alphas[x, z, a] = 0;
                    }
                }
            }

            int rad = TraderTerrainConstants.PatchRadius;

            for (int x = patchXPos - rad; x <= patchXPos + rad; x++)
            {
                int px = x - patchXPos + rad;
                for (int z = patchZPos - rad; z <= patchZPos + rad; z++)
                {
                    int pz = z - patchZPos + rad;
                    int npz = px;
                    int npx = pz;
                    try
                    {
                        patch.Heights[npx, npz] = _heightNoise[x, z] * _heightNoiseScale;

                        int colorIndex = _colorIndexes[x, z];

                        patch.Alphas[npx, npz, colorIndex - 1] = 1;

                    }
                    catch (Exception ee)
                    {
                        _logService.Exception(ee, "TraderTerrain.Draw");
                    }
                }
            }

            await Awaitable.NextFrameAsync(GetToken());
            patch.Core.TerrainData.SetHeights(0, 0, patch.Heights);
            await Awaitable.NextFrameAsync(GetToken());
            patch.Core.TerrainData.SetAlphamaps(0, 0, patch.Alphas);
            await Awaitable.NextFrameAsync(GetToken());

            patch.Core.Terrain.Flush();
            await Awaitable.NextFrameAsync(GetToken());


        }
        catch (Exception ex)
        {
            _logService.Exception(ex, "TraderTerrainShowPatch");
        }

        _patchLoadingCount--;
    }


    private bool _didEverUpdateProps = false;
    private void UpdateProps(int cx, int cz)
    {
        ZoneTypeSettings zoneTypeSettings = _gameData.Get<ZoneTypeSettings>(_gs.ch);
        int minx = (int)(cx - PropVisibilityRadius);
        int maxx = (int)(cx + PropVisibilityRadius);

        int minz = (int)(cz - PropVisibilityRadius);
        int maxz = (int)(cz + PropVisibilityRadius);

        List<CrawlerProp> removeList = new List<CrawlerProp>();

        foreach (CrawlerProp prop in _props)
        {
            float dx = prop.X - cx;
            float dz = prop.Z - cz;

            float dist = Mathf.Sqrt(dx * dx + dz * dz);
            if (dist > PropVisibilityRadius + 0.2f)
            {
                removeList.Add(prop);
            }
        }

        foreach (CrawlerProp prop in removeList)
        {

            _props.Remove(prop);
            _propPool.ReturnObject(prop);
            if (_didEverUpdateProps)
            {
                break;
            }
        }

        for (int x = minx; x <= maxx; x++)
        {
            float dx = x - cx;
            for (int z = minz; z <= maxz; z++)
            {
                float dz = z - cz;

                if (_zoneTypeIds[x, z] < 1)
                {
                    continue;
                }
                float dist = Mathf.Sqrt(dx * dx + dz * dz);

                if (dist <= PropVisibilityRadius)
                {
                    CrawlerProp prop = _props.FirstOrDefault(p => p.X == x && p.Z == z);

                    if (prop == null)
                    {
                        float propChance = _zoneTypePropChances[_zoneTypeIds[x, z]];

                        if (propChance == 0)
                        {
                            continue;
                        }

                        float propSeed = (x * 137 + z * 71 + (x + 1) * (z + 3)) * 0.1245f;

                        int propSeedInt = (int)propSeed;

                        float propSeedRemainder = propSeed - propSeedInt;

                        if (propSeedRemainder > propChance)
                        {
                            continue;
                        }

                        int propSeedOffset = (int)(propSeed) * 17;

                        ZoneType zoneType = zoneTypeSettings.Get(_zoneTypeIds[x, z]);

                        WeightedObject obj = zoneType.Props[propSeedOffset % zoneType.Props.Count];

                        if (obj.EntityTypeId == EntityTypes.Prop)
                        {
                            PropType propType = _gameData.Get<PropTypeSettings>(_gs.ch).Get(obj.EntityId);

                            propSeedOffset *= 19;

                            string propArtName = propType.Art + (1 + propSeedOffset % Math.Max(1, propType.NumChoices));


                            int angle = (int)(propSeed * 17) % 360;

                            PropPosition patchPos = new PropPosition()
                            {
                                Pos = new Vector3(x, _heightNoise[x, z] * _heightNoiseScale, z),
                                Scale = Vector3.one * 0.2f,
                                EulerAngles = new Vector3(0, angle, 0),
                                X = x,
                                Z = z,
                            };

                            _propPool.CheckoutObject(PropAnchor, AssetCategoryNames.Props, propArtName, OnDownloadProp, patchPos, GetToken());

                            if (_didEverUpdateProps)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }


        _didEverUpdateProps = true;
    }

    private void OnDownloadProp(GameObject go, PropPosition data, CancellationToken token)
    {

        if (go == null)
        {
            return;
        }
        CrawlerProp prop = go.GetComponent<CrawlerProp>();

        if (prop == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        prop.X = data.X;
        prop.Z = data.Z;
        go.transform.position = data.Pos;

        go.transform.localScale = data.Scale;
        go.transform.eulerAngles = data.EulerAngles;

        _props.Add(prop);
    }
}




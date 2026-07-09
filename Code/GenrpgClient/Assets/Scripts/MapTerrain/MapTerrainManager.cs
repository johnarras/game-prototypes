using Assets.Scripts.Assets.Constants;
using Assets.Scripts.GameObjects;
using Assets.Scripts.MapTerrain;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IMapTerrainManager : IInitializable
{
    GameObject GetTerrainProtoObject(string name);
    void AddTerrainProtoPatch(string name, int gx, int gz);
    void RemovePatchFromPrototypes(int gx, int gz);
    void Clear();
    Awaitable SetupOneTerrainPatch(int gx, int gz, CancellationToken token);
    bool AddingPatches();
    List<Terrain> GetTerrains();
    TerrainPatchData GetPatchFromMapPos(float worldx, float worldz);
    TerrainPatchData GetMapGrid(int gx, int gz);
    Awaitable AddPatchObjects(int gx, int gz, CancellationToken token);
    void ClearPatches();
    GameObject GetPrototypeParent();
    GameObject AddOrReuseTerrainProtoObject(string name, GameObject go);
    void ClearMapObjects();
    void SetFastLoading();
    long GetPatchesRemoved();
    long GetPatchesAdded();
    void IncrementPatchesAdded();
    bool IsLoadingPatches();
    void RemoveLoadingPatches(int gx, int gz);
    Vector3 GetInterpolatedNormal(Map map, float x, float z);
    float SampleHeight(float x, float z);
    float GetInterpolatedHeight(float xpos, float zpos);
    TerrainPatchData GetTerrainPatch(int gx, int gz, bool createIfNotExist = false);
    void SetTerrainPatchAtGridLocation(int xgrid, int zgrid, Map map, TerrainPatchData data);
    void SetOneTerrainNeighbors(int gx, int gz);
    float GetSteepness(float xpos, float zpos);
    TerrainData GetTerrainData(int gx, int gz);
    void SetAllTerrainNeighbors();
    IMMOMapObjectLoader GetLoader(long entityTypeId);
    Awaitable InitTerrainContainer(ITerrainContainer patch, CancellationToken token);
}





public class PatchLoadData
{
    public TerrainPatchData patch = null;
    public List<ObjectPrototype> objectProtos = new List<ObjectPrototype>();
    public List<TreeInstance> treeInstances = new List<TreeInstance>();
    public GameObject protoParent = null;
    public int StartX = 0;
    public int StartZ = 0;
    public int gx = 0;
    public int gz = 0;

    public IMapTerrainManager terrManager;

    public int MapX(int x)
    {
        return x + StartX;
    }

    public int MapZ(int z)
    {
        return z + StartZ;
    }


}

public delegate void AfterObjectLoad(GameObject go, DownloadObjectData data, CancellationToken token);

public class DownloadObjectData
{
    public IClientGameState ugs;
    public Zone zone;
    public ZoneType zoneType;
    public string url;
    public IIndexedGameItem gameItem;
    public PatchLoadData loadData;
    public int x;
    public int z;
    public float finalY;
    public float zOffset;
    public Point3F rotation;
    public object data;
    public string assetCategory;
    public TreeInstance instance;
    public TreePrototype prototype;
    public TerrainData tdata;
    public float scale = 1.0f;
    public bool allowRandomPlacement = true;


    public AfterObjectLoad AfterLoad;

    public long placementSeed;
    public float height;
    public float ddx;
    public float ddz;

}

public class ObjectPrototype
{
    public string Name;
    public TreePrototype Prototype;
    public IIndexedGameItem DataItem;
    public IMapTerrainManager terrManager;
    public CancellationToken token;
}

public class TerrainProtoObject
{
    public string Name;
    public GameObject Prefab;
    public List<int> PatchIds = new List<int>();
}


public class MapTerrainManager : IMapTerrainManager
{

    private const int MaxLoadUnloadCheckTicks = 13;
    private const int MaxPatchLoadTicks = 23;

    private const int LoadObjectCountBeforePause = 20;

    // Used to move world objects out of the way when we enter a dungeon.


    private SetupDictionaryContainer<long, IMMOMapObjectLoader> _mapObjectLoaders = new SetupDictionaryContainer<long, IMMOMapObjectLoader>();


    private GameObject _prototypeParent = null;

    protected IZoneGenService _zoneGenService = null;
    private ITerrainPatchLoader _patchLoader;
    private IPlayerManager _playerManager;
    protected IMapProvider _mapProvider;
    protected IMapGenData _md;
    private IClientUpdateService _updateService = null;
    private IClientGameState _gs;
    private IAssetService _assetService = null;
    private IGameData _gameData;
    private IClientEntityService _clientEntityService = null;
    private ISingletonContainer _singletonContainer;
    private ITerrainTextureManager _terrainTextureManager = null;

    private List<Point2I> _addPatchList = new List<Point2I>();
    private List<Point2I> _removePatchList = new List<Point2I>();
    private List<Point2I> _loadingPatchList = new List<Point2I>();

    public TerrainPatchData[,] _terrainPatches = new TerrainPatchData[MapConstants.MaxTerrainGridSize, MapConstants.MaxTerrainGridSize];

    private Dictionary<string, TerrainProtoObject> _terrainProtoObjectData = new Dictionary<string, TerrainProtoObject>();

    public async Task Initialize(CancellationToken token)
    {
        _updateService.AddTokenUpdate(this, TerrainUpdate, UpdateTypes.Regular, token);
        _prototypeParent = _singletonContainer.GetAssetParent<ObjectPrototype>();

        await Task.CompletedTask;
    }

    private bool _fastLoading = false;
    public void SetFastLoading()
    {
        _fastLoading = true;
    }


    private long _patchesAdded = 0;
    private long _patchesRemoved = 0;

    public long GetPatchesAdded()
    {
        return _patchesAdded;
    }

    public long GetPatchesRemoved()
    {
        return _patchesRemoved;
    }

    public void IncrementPatchesAdded()
    {
        _patchesAdded++;
    }

    public GameObject GetTerrainProtoObject(string name)
    {
        if (_terrainProtoObjectData.ContainsKey(name))
        {
            return _terrainProtoObjectData[name].Prefab;
        }

        return null;
    }


    private int GetProtoPatchIndex(int gx, int gz)
    {
        return 1000 * gx + gz;
    }

    public bool IsLoadingPatches()
    {
        return _addPatchList?.Count > 0 ||
            _loadingPatchList?.Count > 0;
    }

    public void RemoveLoadingPatches(int gx, int gz)
    {
        if (_loadingPatchList != null)
        {
            _loadingPatchList = _loadingPatchList.Where(x => x.X != gx || x.Z != gz).ToList();
        }
    }

    public void AddTerrainProtoPatch(string name, int gx, int gz)
    {
        int index = GetProtoPatchIndex(gx, gz);

        if (!_terrainProtoObjectData.ContainsKey(name))
        {
            _terrainProtoObjectData[name] = new TerrainProtoObject() { Name = name };
        }

        TerrainProtoObject tpo = _terrainProtoObjectData[name];

        if (!tpo.PatchIds.Contains(index))
        {
            tpo.PatchIds.Add(index);
        }
    }

    public void RemovePatchFromPrototypes(int gx, int gz)
    {

        int index = GetProtoPatchIndex(gx, gz);

        List<string> prefabRemoveList = new List<string>();

        foreach (TerrainProtoObject val in _terrainProtoObjectData.Values)
        {
            if (val.PatchIds.Contains(index))
            {
                val.PatchIds.Remove(index);
                if (val.PatchIds.Count < 1)
                {
                    prefabRemoveList.Add(val.Name);
                }
            }
        }

        foreach (string prefabName in prefabRemoveList)
        {
            _clientEntityService.Destroy(_terrainProtoObjectData[prefabName].Prefab);
            _terrainProtoObjectData.Remove(prefabName);
        }

    }

    public void Clear()
    {
        _terrainProtoObjectData = new Dictionary<string, TerrainProtoObject>();
        _clientEntityService.DestroyAllChildren(_prototypeParent);

        _loadingPatchList = new List<Point2I>();
        _addPatchList = new List<Point2I>();
        _removePatchList = new List<Point2I>();
        ClearMapObjects();
        ClearPatches();
    }

    public GameObject AddOrReuseTerrainProtoObject(string name, GameObject go)
    {
        if (string.IsNullOrEmpty(name) || go == null)
        {
            return null;
        }

        if (_terrainProtoObjectData.ContainsKey(name))
        {
            TerrainProtoObject tpObject = _terrainProtoObjectData[name];

            if (tpObject.Prefab != null)
            {
                _clientEntityService.Destroy(go);
                return tpObject.Prefab;
            }
            else
            {
                tpObject.Prefab = go;
                return go;
            }
        }
        else
        {
            TerrainProtoObject tpo = new TerrainProtoObject()
            {
                Name = name,
                Prefab = go,
            };
            _terrainProtoObjectData[name] = tpo;
            return tpo.Prefab;
        }
    }

    public IMMOMapObjectLoader GetLoader(long entityTypeId)
    {
        if (_mapObjectLoaders.TryGetValue(entityTypeId, out IMMOMapObjectLoader loader))
        {
            return loader;
        }
        return null;
    }

    void TerrainUpdate(CancellationToken token)
    {
        UpdatePatchVisibility(token);
    }

    private float _baseVisibilityRadius = MapConstants.TerrainBlockVisibilityRadius;
    private float _currVisibilityRadus = MapConstants.TerrainBlockVisibilityRadius;
    public float GetVisbilityRadius()
    {
        float rad = _currVisibilityRadus;
        return rad;
    }

    public void SetVisibilityRadiusScale(float scale)
    {
        scale = MathUtil.Clamp(0.1f, scale, 1);
        _currVisibilityRadus = _baseVisibilityRadius * scale;
        if (_currVisibilityRadus < 1.4f)
        {
            _currVisibilityRadus = 1.0f;
        }
    }



    protected bool ListContainsCell(List<Point2I> list, int gx, int gz)
    {
        if (list == null)
        {
            return false;
        }

        return (list.FirstOrDefault(x => x.X == gx && x.Z == gz) != null);
    }

    protected void AddListCell(List<Point2I> list, int gx, int gz)
    {
        if (list == null)
        {
            return;
        }

        {
            list.Add(new Point2I(gx, gz));
        }
    }

    protected void RemoveListCell(List<Point2I> list, int gx, int gz)
    {
        if (list == null)
        {
            return;
        }

        Point2I item = list.FirstOrDefault(x => x.X == gx && x.Z == gz);
        if (item != null)
        {
            list.Remove(item);
        }
    }


    public bool AddingPatches()
    {
        if (_addPatchList.Count > 0 ||
            _loadingPatchList.Count > 0 ||
            _removePatchList.Count > 0)
        {
            return true;
        }

        return false;
    }


    int loadUnloadCheckTicks = 0;
    int patchCheckTicks = 0;

    void UpdatePatchVisibility(CancellationToken token)
    {
        if (!_playerManager.Exists())
        {
            return;
        }

        if (_mapProvider.GetMap() == null)
        {
            return;
        }

        loadUnloadCheckTicks--;
        patchCheckTicks--;

        if (_fastLoading)
        {
            loadUnloadCheckTicks = 0;
            patchCheckTicks = 0;
        }

        if (loadUnloadCheckTicks <= 0)
        {
            if (_playerManager.TryGetUnit(out Unit unit))
            {

                Point3F ppos = unit.GetPos();
                Vector3 playerPos = new Vector3(ppos.X, ppos.Z, ppos.Z);

                loadUnloadCheckTicks = MaxLoadUnloadCheckTicks;

                int XGrid = (int)((playerPos.x + MapConstants.TerrainPatchSize / 2) / (MapConstants.TerrainPatchSize - 1));
                int ZGrid = (int)((playerPos.z + MapConstants.TerrainPatchSize / 2) / (MapConstants.TerrainPatchSize - 1));


                float loadRad = GetVisbilityRadius() + 0.25f;
                float unloadRad = loadRad + 1.0f;
                float checkRad = unloadRad + 2.0f;

                if (_gs.Rand.NextDouble() < 0.2f)
                {
                    checkRad = _mapProvider.GetMap().BlockCount + 1;
                }

                int minx = (int)Math.Max(0, XGrid - checkRad);
                int maxx = (int)Math.Min(_mapProvider.GetMap().BlockCount - 1, XGrid + checkRad);
                int minz = (int)Math.Max(0, ZGrid - checkRad);
                int maxz = (int)Math.Min(_mapProvider.GetMap().BlockCount - 1, ZGrid + checkRad);

                for (int x = 0; x < _mapProvider.GetMap().BlockCount; x++)
                {
                    // Loop over all x in the middle range, or the full row check range.
                    if (x < minx || x > maxx)
                    {
                        continue;
                    }
                    // continue;
                    for (int z = minz; z <= maxz; z++)
                    {
                        // If we are on the full row check and not in the main middle block,
                        // only check that one row.
                        if (z < minz || z > maxz)
                        {
                            continue;
                        }
                        float dx = playerPos.x - (x + 0.5f) * (MapConstants.TerrainPatchSize - 1);
                        float dy = playerPos.z - (z + 0.5f) * (MapConstants.TerrainPatchSize - 1);

                        float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                        // Check if we load the patch if it's near the player's position.
                        if (dist < loadRad * (MapConstants.TerrainPatchSize - 1))
                        {
                            TerrainPatchData patch = GetTerrainPatch(x, z);
                            if (patch != null)
                            {
                                Terrain terr = patch.Core.Terrain;
                                if (terr == null)
                                {
                                    if (!ListContainsCell(_addPatchList, x, z) &&
                                        !ListContainsCell(_loadingPatchList, x, z))
                                    {
                                        AddListCell(_addPatchList, x, z);
                                        RemoveListCell(_removePatchList, x, z);
                                    }
                                }
                            }
                        }
                        // Check if we unload the patch.
                        else if (dist > unloadRad * (MapConstants.TerrainPatchSize - 1))
                        {
                            TerrainPatchData patch = GetTerrainPatch(x, z, false);
                            if (patch != null)
                            {
                                Terrain terr = patch.Core.Terrain;
                                if (terr != null)
                                {
                                    if (!ListContainsCell(_loadingPatchList, x, z))
                                    {
                                        RemoveListCell(_addPatchList, x, z);
                                        if (!ListContainsCell(_removePatchList, x, z) && terr != null)
                                        {
                                            AddListCell(_removePatchList, x, z);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (patchCheckTicks <= 0)
        {
            patchCheckTicks = MaxPatchLoadTicks;
            if (_loadingPatchList.Count > 2)
            {
                return;
            }

            // Add patches if there are any to add if it's normal speed or
            // we have nothing to remove in fast remove mode.
            if (_addPatchList.Count > 0)
            {

                int loadTimes = (_fastLoading ? 100 : 1);

                for (int tt = 0; tt < loadTimes; tt++)
                {
                    if (_addPatchList.Count < 1)
                    {
                        break;
                    }

                    Point2I firstItem = _addPatchList[0];
                    _addPatchList.RemoveAt(0);
                    _loadingPatchList.Add(firstItem);

                    _patchLoader.LoadOneTerrainPatch(firstItem.X, firstItem.Z, _fastLoading, token);
                }
            }

            if (_removePatchList.Count > 0)
            {
                Point2I firstItem = _removePatchList[0];
                _removePatchList.Remove(firstItem);
                _patchesRemoved++;
                TerrainPatchData patch = GetTerrainPatch(firstItem.X, firstItem.Z);
                if (patch == null)
                {
                    return;
                }
                RemoveTerrainPatch(firstItem.X, firstItem.Z);
                Terrain terr = patch.Core.Terrain;

                if (terr != null)
                {
                    if (terr.terrainData != null && terr.terrainData.terrainLayers != null)
                    {
                        terr.terrainData.treeInstances = new TreeInstance[0];

                        terr.terrainData.treePrototypes = new TreePrototype[0];
                        terr.terrainData.RefreshPrototypes();

                        RemovePatchFromPrototypes(firstItem.X, firstItem.Z);

                        for (int layerIdx = 0; layerIdx < terr.terrainData.terrainLayers.Length; layerIdx++)
                        {
                            TerrainLayer layer = terr.terrainData.terrainLayers[layerIdx];
                            if (layer.diffuseTexture != null)
                            {
                                string texName = layer.diffuseTexture.name.Replace("_d", "");
                            }
                        }
                    }

                    _clientEntityService.Destroy(terr.gameObject);
                }
                patch.Core.Terrain = null;
                patch.Core.TerrainData = null;
                patch.DataBytes = null;
                patch.grassAmounts = null;
                patch.heights = null;
                patch.overrideZoneScales = null;
                patch.subZoneIds = null;
                patch.mainZoneIds = null;
                patch.baseAlphas = null;
                patch.entityIds = null;
                patch.entityTypeIds = null;
                patch.FullZoneIdList = null;
                patch.MainZoneIdList = null;
            }
        }

        if (_fastLoading && _addPatchList.Count < 1 && _loadingPatchList.Count < 1)
        {
            _fastLoading = false;
        }
    }


    public void ClearPatches()
    {
        if (_terrainPatches == null)
        {
            return;
        }

        for (int x = 0; x < _terrainPatches.GetLength(0); x++)
        {
            for (int z = 0; z < _terrainPatches.GetLength(1); z++)
            {
                if (_terrainPatches[x, z] != null)
                {
                    Terrain terr = _terrainPatches[x, z].Core.Terrain;
                    if (terr != null)
                    {
                        _clientEntityService.Destroy(terr.gameObject);
                    }
                }
            }
        }
        _terrainPatches = new TerrainPatchData[MapConstants.MaxTerrainGridSize, MapConstants.MaxTerrainGridSize];

    }

    public async Awaitable SetupOneTerrainPatch(int gx, int gz, CancellationToken token)
    {
        if (gx < 0 || gz < 0 || gx >= _mapProvider.GetMap().BlockCount ||
            gz >= _mapProvider.GetMap().BlockCount)
        {
            return;
        }
        TerrainPatchData patch = GetTerrainPatch(gx, gz);
        if (patch == null)
        {
            return;
        }
        await InitTerrainContainer(patch, token);
    }

    public async Awaitable InitTerrainContainer(ITerrainContainer cont, CancellationToken token)
    {

        if (cont.Core.Terrain != null && cont.Core.TerrainData != null)
        {
            return;
        }

        int patchSize = cont.Core.TerrainSize;

        float[,] patchHeights = new float[patchSize, patchSize];
        for (int x = 0; x < patchSize; x++)
        {
            for (int z = 0; z < patchSize; z++)
            {
                patchHeights[x, z] = MapConstants.StartHeightPercent;
            }
        }
        int alphaPatchSize = patchSize * MapConstants.AlphaMapsPerTerrainCell;
        float[,,] tempPatchAlphas = new float[alphaPatchSize, alphaPatchSize, TerrainTexChannels.Max];

        for (int x = 0; x < alphaPatchSize; x++)
        {
            for (int z = 0; z < alphaPatchSize; z++)
            {
                tempPatchAlphas[x, z, 0] = 1.0f;
            }
        }

        int gx = cont.Core.GX;
        int gz = cont.Core.GZ;
        Vector3 offsetPos = new Vector3(gx * (patchSize - 1), 0, gz * (patchSize - 1)) * cont.Core.WorldUnitsPerCell;
        string terrainName = "Terrain" + gx + "_" + gz;

        GameObject terrObj2 = (GameObject)(await _assetService.LoadAssetAsync(AssetCategoryNames.Prefabs, "TerrainMaterialPlaceholder", null, token));
        terrObj2.name = terrainName;

        terrObj2.transform.localPosition = offsetPos;
        Terrain terr = terrObj2.GetComponent<Terrain>();
        terr.terrainData.detailPrototypes = new DetailPrototype[0];
        terr.terrainData.treePrototypes = new TreePrototype[0];
        terr.terrainData = GameObject.Instantiate<TerrainData>(terr.terrainData);
        TerrainCollider coll = _clientEntityService.GetOrAddComponent<TerrainCollider>(terrObj2);
        coll.terrainData = terr.terrainData;

        cont.Core.Terrain = terr;
        cont.Core.TerrainData = terr.terrainData;

        if (!_fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
        terr.terrainData.baseMapResolution = (patchSize - 1) / 2;
        terr.terrainData.heightmapResolution = patchSize;
        terr.terrainData.SetHeights(0, 0, patchHeights);

        TerrainLayer[] arr = new TerrainLayer[TerrainTexChannels.Max];
        for (int s = 0; s < arr.Length; s++)
        {
            arr[s] = _terrainTextureManager.CreateTerrainLayer(null);
        }
        terr.terrainData.terrainLayers = arr;

        if (!_fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }

        terr.terrainData.alphamapResolution = patchSize * MapConstants.AlphaMapsPerTerrainCell;
        terr.terrainData.SetDetailResolution(MapConstants.DetailResolution, MapConstants.DetailResolutionPerPatch);

        terr.terrainData.SetAlphamaps(0, 0, tempPatchAlphas);
        terr.Flush();

        float maxHeight = MapConstants.MapHeight;
        terr.terrainData.heightmapResolution = patchSize;
        terr.terrainData.size = new Vector3((patchSize - 1) * cont.Core.WorldUnitsPerCell, maxHeight, (patchSize - 1) * cont.Core.WorldUnitsPerCell);

        terr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Simple;
        terr.treeDistance = 400;
        terr.treeBillboardDistance = 400;
        terr.treeCrossFadeLength = 5;
        terr.treeMaximumFullLODCount = 500;
        terr.basemapDistance = 250;
        terr.heightmapPixelError = 10;
        terr.detailObjectDensity = 0.5f;
        terr.detailObjectDistance = 200;
        terr.drawHeightmap = true;
        terr.drawTreesAndFoliage = true;
        terr.collectDetailPatches = false;
        terr.drawInstanced = true;
        terr.allowAutoConnect = true;
        terr.keepUnusedRenderingResources = true;

    }

    public List<Terrain> GetTerrains()
    {
        List<Terrain> retval = new List<Terrain>();
        if (_terrainPatches == null)
        {
            return retval;
        }

        for (int x = 0; x < _terrainPatches.GetLength(0); x++)
        {
            for (int z = 0; z < _terrainPatches.GetLength(1); z++)
            {
                if (_terrainPatches[x, z] != null)
                {
                    Terrain terr = _terrainPatches[x, z].Core.Terrain;
                    if (terr != null)
                    {
                        retval.Add(terr);
                    }
                }
            }
        }

        return retval;
    }


    public GameObject GetPrototypeParent()
    {
        return _prototypeParent;
    }

    public async Awaitable AddPatchObjects(int gx, int gz, CancellationToken token)
    {
        PatchLoadData loadData = new PatchLoadData();
        loadData.gx = gx;
        loadData.gz = gz;
        loadData.StartX = loadData.gx * (MapConstants.TerrainPatchSize - 1);
        loadData.StartZ = loadData.gz * (MapConstants.TerrainPatchSize - 1);
        loadData.terrManager = this;

        TerrainPatchData patch = GetTerrainPatch(gx, gz);
        loadData.patch = patch;


        if (patch == null || patch.FullZoneIdList == null || patch.FullZoneIdList.Count < 1)
        {
            return;
        }


        Terrain pterr = loadData.patch.Core.Terrain;
        if (pterr != null)
        {
            //loadData.protoParent = pterr.entity();
            loadData.protoParent = _prototypeParent;
        }

        loadData.objectProtos = new List<ObjectPrototype>();
        loadData.treeInstances = new List<TreeInstance>();

        List<Zone> currZones = new List<Zone>();

        foreach (long zid in loadData.patch.FullZoneIdList)
        {
            Zone zn = _mapProvider.GetMap().Get<Zone>(zid);
            if (zn != null)
            {
                currZones.Add(zn);
            }
        }

        List<ZoneType> zoneTypeCache = new List<ZoneType>();

        if (loadData.patch.entityTypeIds == null)
        {
            loadData.patch.entityTypeIds = new byte[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];
        }

        if (loadData.patch.entityIds == null)
        {
            loadData.patch.entityIds = new byte[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];
        }

        int addTimes = 0;

        int currZoneId = -1;
        Zone currZone = null;
        ZoneType currZoneType = null;

        for (int x = 0; x < MapConstants.TerrainPatchSize - 1; x++)
        {
            for (int z = 0; z < MapConstants.TerrainPatchSize - 1; z++)
            {

                if (loadData.patch.entityTypeIds[x, z] == 0)
                {
                    continue;
                }

                int entityTypeId = loadData.patch.entityTypeIds[x, z];
                int entityId = loadData.patch.entityIds[x, z];

                if (loadData.patch.heights == null || loadData.patch.heights[z, x] < MapConstants.StartHeightPercent * 0.75f)
                {
                    continue;
                }

                int zoneId = loadData.patch.mainZoneIds[x, z];


                if (zoneId != currZoneId)
                {
                    currZoneId = zoneId;
                    currZone = currZones.FirstOrDefault(xx => xx.IdKey == zoneId);
                    if (currZone == null)
                    {
                        currZone = _mapProvider.GetMap().Get<Zone>(currZoneId);
                        if (currZone == null)
                        {
                            currZoneId = -1;
                            currZoneType = null;
                            continue;
                        }
                        currZones.Add(currZone);
                    }
                    currZoneType = zoneTypeCache.FirstOrDefault(xx => xx.IdKey == currZone.ZoneTypeId);
                    if (currZoneType == null)
                    {
                        currZoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(currZone.ZoneTypeId);
                        if (currZoneType == null)
                        {
                            currZoneId = -1;
                            currZone = null;
                            continue;
                        }
                        zoneTypeCache.Add(currZoneType);
                    }
                }

                if (currZone == null || currZoneType == null)
                {
                    continue;
                }

                IMMOMapObjectLoader loader = GetLoader(entityTypeId);


                if (loader == null)
                {
                    continue;
                }

                loader.LoadObject(loadData, entityId, x, z, currZone, currZoneType, token);

                addTimes++;
                if (addTimes >= LoadObjectCountBeforePause)
                {
                    addTimes = 0;
                    if (!_fastLoading)
                    {
                        await Awaitable.NextFrameAsync(cancellationToken: token);
                    }
                }
                if (_terrainPatches[loadData.gx, loadData.gz] == null)
                {
                    return;
                }
            }
        }


        await Awaitable.NextFrameAsync(cancellationToken: token);

        // Wait until all protos have been downloaded.
        while (true)
        {
            bool haveAllProtos = true;
            for (int p = 0; p < loadData.objectProtos.Count; p++)
            {
                if (loadData.objectProtos[p].Prototype.prefab == null)
                {
                    haveAllProtos = false;
                    break;
                }
            }

            if (haveAllProtos)
            {
                break;
            }

            await Awaitable.NextFrameAsync(cancellationToken: token);

        }

        if (_md != null && loadData != null && loadData.patch != null && _terrainPatches[loadData.gx, loadData.gz] != null)
        {
            TerrainData tdata = loadData.patch.Core.TerrainData as TerrainData;
            Terrain terr = loadData.patch.Core.Terrain;
            if (tdata != null && terr != null)
            {
                TreePrototype[] treeProtos = new TreePrototype[loadData.objectProtos.Count];
                for (int p = 0; p < loadData.objectProtos.Count; p++)
                {
                    treeProtos[p] = loadData.objectProtos[p].Prototype;
                }

                tdata.treePrototypes = treeProtos;
                tdata.RefreshPrototypes();


                await Awaitable.NextFrameAsync(cancellationToken: token);

                TreeInstance[] tarray = loadData.treeInstances.ToArray();


                if (tdata != null)
                {
                    tdata.SetTreeInstances(tarray, false);
                    terr.Flush();
                }

                // Disable and enable this AFTER adding the tree instances so that the tree
                // colliders will work!
                TerrainCollider tcol = terr.GetComponent<TerrainCollider>();
                if (tcol != null)
                {
                    tcol.enabled = false;
                    tcol.enabled = true;
                }
            }
        }
    }


    int gridPosX = 0;
    int gridPosY = 0;
    public TerrainPatchData GetPatchFromMapPos(float worldx, float worldz)
    {
        gridPosX = (int)(worldx / (MapConstants.TerrainPatchSize - 1));
        gridPosY = (int)(worldz / (MapConstants.TerrainPatchSize - 1));
        return GetMapGrid(gridPosX, gridPosY);
    }

    public TerrainPatchData GetMapGrid(int gx, int gz)
    {
        if (gx < 0 || gz < 0 || gx >= MapConstants.MaxTerrainGridSize || gz >= MapConstants.MaxTerrainGridSize)
        {
            return null;
        }
        return _terrainPatches[gx, gz];
    }





    public void ClearMapObjects()
    {
        if (_terrainPatches == null)
        {
            return;
        }

        for (int x = 0; x < _terrainPatches.GetLength(0); x++)
        {
            for (int z = 0; z < _terrainPatches.GetLength(1); z++)
            {
                TerrainPatchData patch = _terrainPatches[x, z];
                if (patch == null)
                {
                    continue;
                }

                Terrain terr = patch.Core.Terrain;
                if (terr == null)
                {
                    continue;
                }

                _clientEntityService.Destroy(terr.gameObject);
                _terrainPatches[x, z] = null;

            }
        }
    }


    public Vector3 GetInterpolatedNormal(Map map, float x, float z)
    {
        const float NormalEdgePct = 0.001f;
        int divSize = MapConstants.TerrainPatchSize - 1;
        int normalXGrid = 0;
        int normalZGrid = 0;
        TerrainPatchData normalPatch = null;
        TerrainData normalTerrainData = null;
        if (!_md.HaveSetHeights)
        {
            throw new Exception("You must set the terrain heights before interpolating height.");
        }

        float startx = x;
        float startz = z;

        normalXGrid = (int)(x / (divSize));
        normalZGrid = (int)(z / (divSize));

        x -= normalXGrid * (divSize);
        z -= normalZGrid * (divSize);

        if (normalXGrid < 0 || normalZGrid < 0 || normalXGrid >= map.BlockCount || normalZGrid >= map.BlockCount ||
            _terrainPatches == null)
        {
            return Vector3.up;
        }

        normalPatch = _terrainPatches[normalXGrid, normalZGrid];

        if (normalPatch == null)
        {
            return Vector3.up;
        }

        normalTerrainData = normalPatch.Core.TerrainData as TerrainData;

        if (normalTerrainData == null)
        {
            return Vector3.up;
        }
        x = MathUtil.Clamp(NormalEdgePct, x / (divSize), 1 - NormalEdgePct);
        z = MathUtil.Clamp(NormalEdgePct, z / (divSize), 1 - NormalEdgePct);
        Vector3 norm = normalTerrainData.GetInterpolatedNormal(x, z);

        return norm;
    }
    public float GetSteepness(float xpos, float zpos)
    {
        if (!_md.HaveSetHeights)
        {
            return 0.0f;
            //throw new Exception("Tried to calc steepness before setting heights!");
        }


        int xgrid = (int)(xpos / (MapConstants.TerrainPatchSize - 1));
        int zgrid = (int)(zpos / (MapConstants.TerrainPatchSize - 1));

        float localx = xpos - xgrid * (MapConstants.TerrainPatchSize - 1);
        float localz = zpos - zgrid * (MapConstants.TerrainPatchSize - 1);

        if (localx < 0.1f)
        {
            localx += 0.1f;
        }

        if (localz < 0.1f)
        {
            localz += 0.1f;
        }

        TerrainData tdata2 = GetTerrainData(xgrid, zgrid);

        if (tdata2 == null)
        {
            return 0.0f;
        }

        float endDelta = 0.2f;
        localx = MathUtil.Clamp(endDelta, localx, MapConstants.TerrainPatchSize - 1 - endDelta);
        localz = MathUtil.Clamp(endDelta, localz, MapConstants.TerrainPatchSize - 1 - endDelta);

        return tdata2.GetSteepness((localx + 0.0f) / (MapConstants.TerrainPatchSize - 1), (localz + 0.0f) / (MapConstants.TerrainPatchSize - 1));
    }

    public float GetInterpolatedHeight(float xpos, float zpos)
    {
        int interpXGrid = 0;
        int interpZGrid = 0;
        float interpLocalX = 0;
        float interpLocalZ = 0;
        TerrainData normalTerrainData = null;
        if (!_md.HaveSetHeights)
        {
            return 0.0f;
        }
        interpXGrid = (int)(xpos / (MapConstants.TerrainPatchSize - 1));
        interpZGrid = (int)(zpos / (MapConstants.TerrainPatchSize - 1));

        interpLocalX = xpos - interpXGrid * (MapConstants.TerrainPatchSize - 1);
        interpLocalZ = zpos - interpZGrid * (MapConstants.TerrainPatchSize - 1);

        if (interpLocalX < 0.1f)
        {
            interpLocalX += 0.1f;
        }

        if (interpLocalZ < 0.1f)
        {
            interpLocalZ += 0.1f;
        }

        normalTerrainData = GetTerrainData(interpXGrid, interpZGrid);

        if (normalTerrainData == null)
        {
            return 0.0f;
        }


        interpLocalX = MathUtil.Clamp(0.01f, interpLocalX, MapConstants.TerrainPatchSize - 1.01f);
        interpLocalZ = MathUtil.Clamp(0.01f, interpLocalZ, MapConstants.TerrainPatchSize - 1.01f);

        return normalTerrainData.GetInterpolatedHeight((interpLocalX + 0.0f) / MapConstants.TerrainPatchSize, (interpLocalZ + 0.0f) / MapConstants.TerrainPatchSize);
    }

    int sampleXGrid = 0;
    int sampleYGrid = 0;
    Terrain sampleTerrain = null;
    public float SampleHeight(float x, float z)
    {
        if (!_md.HaveSetHeights)
        {
            return 0.0f;
        }


        sampleXGrid = (int)(x / (MapConstants.TerrainPatchSize - 1));
        sampleYGrid = (int)(z / (MapConstants.TerrainPatchSize - 1));


        sampleTerrain = GetTerrain(sampleXGrid, sampleYGrid);
        if (sampleTerrain == null)
        {
            return 0.0f;
        }



        return sampleTerrain.SampleHeight(new Vector3(x, MapConstants.MapHeight, z));
    }


    TerrainPatchData getDataPatch = null;
    public TerrainData GetTerrainData(int gx, int gz)
    {
        getDataPatch = GetTerrainPatch(gx, gz);
        if (getDataPatch == null)
        {
            return null;
        }
        return getDataPatch.Core.TerrainData as TerrainData;

    }


    public void SetAllTerrainNeighbors()
    {
        for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
        {
            for (int gz = 0; gz < _mapProvider.GetMap().BlockCount; gz++)
            {
                SetOneTerrainNeighbors(gx, gz);
            }
        }
    }


    TerrainPatchData getTerrainPatch = null;
    public Terrain GetTerrain(int gx, int gz)
    {
        getTerrainPatch = GetTerrainPatch(gx, gz);
        if (getTerrainPatch == null)
        {
            return null;
        }
        return getTerrainPatch.Core.Terrain;
    }

    public void SetOneTerrainNeighbors(int gx, int gz)
    {
        if (gx < 0 || gz < 0)
        {
            return;
        }

        Terrain mid = GetTerrain(gx, gz);
        if (mid == null)
        {
            return;
        }
        Terrain top = GetTerrain(gx, gz + 1);
        Terrain bottom = GetTerrain(gx, gz - 1);
        Terrain left = GetTerrain(gx - 1, gz);
        Terrain right = GetTerrain(gx + 1, gz);
        mid.SetNeighbors(left, top, right, bottom);
    }

    public int GetHeightmapSize()
    {
        if (_mapProvider.GetMap() == null || _mapProvider.GetMap().BlockCount < 4)
        {
            return MapConstants.DefaultHeightmapSize;
        }
        return _mapProvider.GetMap().GetMapSize();
    }


    public TerrainPatchData GetTerrainPatch(int gx, int gz, bool createIfNotThere = true)
    {
        if (gx < 0 || gz < 0 ||
            _terrainPatches == null ||
            gx >= MapConstants.MaxTerrainGridSize ||
            gz >= MapConstants.MaxTerrainGridSize)
        {
            return null;
        }
        if (_terrainPatches[gx, gz] == null)
        {
            SetTerrainPatchAtGridLocation(gx, gz, _mapProvider.GetMap(), null);
        }
        return _terrainPatches[gx, gz];
    }

    public void RemoveTerrainPatch(int gx, int gz)
    {
        if (gx < 0 || gz < 0 || _terrainPatches == null ||
            gx >= MapConstants.MaxTerrainGridSize || gz >= MapConstants.MaxTerrainGridSize)
        {
            return;
        }
        _terrainPatches[gx, gz] = null;
    }

    public void SetTerrainPatchAtGridLocation(int xgrid, int zgrid, Map map, TerrainPatchData data)
    {
        if (xgrid < 0 || zgrid < 0 || xgrid >= MapConstants.MaxTerrainGridSize || zgrid >= MapConstants.MaxTerrainGridSize ||
           map == null)
        {
            return;
        }

        TerrainPatchData oldPatch = _terrainPatches[xgrid, zgrid];

        if (data == null)
        {
            data = new TerrainPatchData();
        }

        data.MapId = map.Id;
        data.MapVersion = map.MapVersion;
        data.Core.GX = xgrid;
        data.Core.GZ = zgrid;
        data.Core.TerrainSize = MapConstants.TerrainPatchSize;
        if (map != null)
        {
            data.MapId = map.Id;
        }
        _terrainPatches[xgrid, zgrid] = data;
        if (oldPatch != null)
        {
            data.Core.Terrain = oldPatch.Core.Terrain;
            data.Core.TerrainData = oldPatch.Core.TerrainData;
        }
    }

}



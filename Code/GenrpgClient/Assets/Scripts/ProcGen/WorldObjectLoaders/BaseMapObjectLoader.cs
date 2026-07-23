using OxDb.Client.Awaitables;
using OxDb.Client.GameObjects;
using OxDb.Client.MapTerrain;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using OxDb.SharedGame.MapServer.Services;
using System.Threading;
using UnityEngine;

/// <summary>
/// Base class for object loaders
/// 
/// *** NOTE IF YOU MAKE A NEW ONE OF THESE YOU MUST REGISTER IT IN Map
/// </summary>
public abstract class BaseCrawlerMapObjectLoader : ICrawlerMapObjectLoader
{
    public abstract long HelperKey { get; }

    public abstract Awaitable Load(OnSpawn message, MapObject loadedObject, CancellationToken token);

    protected abstract string GetLayerName();

    protected IMapTerrainManager _terrainManager;
    protected IAssetService _assetService = null;
    protected IClientMapObjectManager _objectManager;
    protected IGameData _gameData;
    protected IMapProvider _mapProvider;
    protected IClientGameState _gs;
    protected IClientEntityService _clientEntityService = null;
    protected IAwaitableService _awaitableService = null;

    public void FinalPlaceObject(GameObject go, SpawnLoadData data, string layerName)
    {
        if (go == null)
        {
            return;
        }

        TerrainPatchData patchData = _terrainManager.GetPatchFromMapPos(data.Spawn.X, data.Spawn.Z);

        if (patchData == null)
        {
            return;
        }
        Terrain terrain = patchData.Core.Terrain;
        if (terrain != null)
        {
            _clientEntityService.AddToParent(go, terrain.gameObject);
        }
        else
        {
            _clientEntityService.Destroy(go);
            return;
        }

        _clientEntityService.SetLayer(go, LayerUtils.NameToLayer(layerName));

        long placementSeed = (long)(data.Spawn.X * 131 + data.Spawn.Z * 517);

        float nx = RandUtils.SeedFloatRange(placementSeed * 13, 143, -0.5f, 0.5f, 101) + data.Spawn.X;
        float nz = RandUtils.SeedFloatRange(placementSeed * 17, 149, -0.5f, 0.5f, 101) + data.Spawn.Z;

        if (data.FixedPosition)
        {
            nx = data.Spawn.X;
            nz = data.Spawn.Z;
        }

        float height = _terrainManager.SampleHeight(nx, nz);

        go.transform.position = new Vector3(nx, height, nz);
        go.transform.eulerAngles = new Vector3(0, data.Spawn.Rot, 0);

        if (data.Obj is Character ch)
        {
            go.transform.position += new Vector3(0, 2, 0);
        }

        _objectManager.AddObject(data.Obj, go);
    }
}


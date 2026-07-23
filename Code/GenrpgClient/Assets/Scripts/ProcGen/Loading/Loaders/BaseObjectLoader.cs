using OxDb.Client.GameObjects;
using OxDb.Client.MapTerrain;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Constants;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Threading;
using UnityEngine;


public interface IMMOMapObjectLoader : ISetupDictionaryItem<long>
{
    bool LoadObject(PatchLoadData loadData, int entityId, int x, int z,
        Zone currZone, ZoneType currZoneType, CancellationToken token);

    void FinalPlaceObject(GameObject go, DownloadObjectData dlo, CancellationToken token);
}

public abstract class BaseObjectLoader : IMMOMapObjectLoader
{
    protected IAssetService _assetService = null;
    protected IMapTerrainManager _terrainManager;
    protected IGameData _gameData;
    protected IMapProvider _mapProvider;
    protected IClientGameState _gs;
    protected IMapGenData _md;
    protected IClientEntityService _clientEntityService = null;
    protected ILogService _logService = null;

    public abstract long HelperKey { get; }

    public abstract bool LoadObject(PatchLoadData loadData, int entityId, int x, int z,
        Zone currZone, ZoneType currZoneType, CancellationToken token);

    protected void OnDownloadObject(GameObject go, DownloadObjectData data, CancellationToken token)
    {
        FinalPlaceObject(go, data, token);
    }

    public virtual void FinalPlaceObject(GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        if (go == null)
        {
            return;
        }

        if (dlo == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        if (dlo == null || dlo.loadData == null || dlo.loadData.patch == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }


        int gx = dlo.loadData.gx;
        int gz = dlo.loadData.gz;
        int wx = gx * (MapConstants.TerrainPatchSize - 1) + dlo.x;
        int wz = gz * (MapConstants.TerrainPatchSize - 1) + dlo.z;

        TerrainPatchData patch = dlo.loadData.patch;

        Terrain terr = patch.Core.Terrain;
        if (terr == null)
        {
            return;
        }

        GameObject terrGo = terr.gameObject;

        if (terrGo == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        _clientEntityService.AddToParent(go, terrGo);
        _clientEntityService.SetLayer(go, LayerUtils.NameToLayer(LayerNames.ObjectLayer));

        dlo.placementSeed = 17041 + dlo.x * 9479 + dlo.z * 2281 + dlo.loadData.gx * 5281 + dlo.loadData.gz * 719
            + dlo.loadData.gx * dlo.z + dlo.loadData.gz * dlo.x;


        if (dlo.allowRandomPlacement)
        {
            dlo.ddx = RandUtils.SeedFloatRange(dlo.placementSeed * 13, 143, -0.5f, 0.5f, 101);
            dlo.ddz = RandUtils.SeedFloatRange(dlo.placementSeed * 17, 149, -0.5f, 0.5f, 101);
        }
        dlo.height = _terrainManager.SampleHeight(wx, wz);
        go.transform.localPosition = new Vector3(dlo.x + dlo.ddx, dlo.height + dlo.zOffset, dlo.z + dlo.ddz);
        go.transform.localScale = Vector3.one;
        if (dlo.finalY > 0)
        {
            go.transform.localPosition = new Vector3(dlo.x + dlo.ddx, dlo.finalY, dlo.z + dlo.ddz);
        }
        if (dlo.rotation != null)
        {
            go.transform.Rotate(dlo.rotation.X, dlo.rotation.Z, dlo.rotation.Z);
        }
        else
        {
            go.transform.Rotate(0, (dlo.placementSeed * 13) % 360, 0);
        }
        if (dlo.AfterLoad != null)
        {
            dlo.AfterLoad(go, dlo, token);
        }

        if (dlo.scale != 1.0f)
        {
            go.transform.localScale = Vector3.one * dlo.scale;
        }
    }



}



using Assets.Scripts.Core;
using Assets.Scripts.GameObjects;
using Assets.Scripts.MapTerrain;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Constants;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Threading;
using UnityEngine;

public abstract class BaseObjectLoader : IInjectable
{
    protected IAssetService _assetService = null;
    protected IMapTerrainManager _terrainManager;
    protected IGameData _gameData;
    protected IMapProvider _mapProvider;
    protected IClientGameState _gs;
    protected IClientRandom _rand;
    protected IMapGenData _md;
    protected IClientEntityService _clientEntityService = null;

    public abstract bool LoadObject(PatchLoadData loadData, uint objectId, int x, int y,
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
        int gy = dlo.loadData.gy;
        int wx = gx * (MapConstants.TerrainPatchSize - 1) + dlo.x;
        int wy = gy * (MapConstants.TerrainPatchSize - 1) + dlo.y;

        TerrainPatchData patch = dlo.loadData.patch;

        Terrain terr = patch.terrain as Terrain;
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

        dlo.placementSeed = 17041 + dlo.x * 9479 + dlo.y * 2281 + dlo.loadData.gx * 5281 + dlo.loadData.gy * 719
            + dlo.loadData.gx * dlo.y + dlo.loadData.gy * dlo.x;


        if (dlo.allowRandomPlacement)
        {
            dlo.ddx = RandUtils.SeedFloatRange(dlo.placementSeed * 13, 143, -0.5f, 0.5f, 101);
            dlo.ddy = RandUtils.SeedFloatRange(dlo.placementSeed * 17, 149, -0.5f, 0.5f, 101);
        }
        dlo.height = _terrainManager.SampleHeight(wx, wy);
        go.transform.localPosition = new Vector3(dlo.x + dlo.ddx, dlo.height + dlo.zOffset, dlo.y + dlo.ddy);
        go.transform.localScale = Vector3.one;
        if (dlo.finalZ > 0)
        {
            go.transform.localPosition = new Vector3(dlo.x + dlo.ddx, dlo.finalZ, dlo.y + dlo.ddy);
        }
        if (dlo.rotation != null)
        {
            go.transform.Rotate(dlo.rotation.X, dlo.rotation.Y, dlo.rotation.Z);
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



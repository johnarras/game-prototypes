
using Assets.Scripts.Assets.Constants;
using Assets.Scripts.GroundObjects;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Constants;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class MapModObjectLoader : BaseCrawlerMapObjectLoader
{
    public override long HelperKey => EntityTypes.MapMod;
    protected override string GetLayerName() { return LayerNames.ObjectLayer; }

    public override async Awaitable Load(OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        float wx = spawn.X;
        float wz = spawn.Z;

        SpawnLoadData loadData = new SpawnLoadData()
        {
            Obj = obj,
            Spawn = spawn,
            Token = token,
        };

        _assetService.LoadAsset(AssetCategoryNames.Props, "MapMod", OnDownloadMapModObject, null, token, loadData);

        await Task.CompletedTask;
        return;
    }

    private void OnDownloadMapModObject(GameObject go, SpawnLoadData loadData, CancellationToken token)
    {
        MapModObject mapModObject = go.GetComponent<MapModObject>();

        mapModObject.Init(loadData.Spawn);
        FinalPlaceObject(go, loadData, LayerNames.ObjectLayer);
        go.transform.position += new Vector3(0, 1, 0);
    }
}






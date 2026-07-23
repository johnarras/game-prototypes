
using OxDb.Client.Assets.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Constants;
using OxDb.SharedGame.GroundObjects.Settings;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using OxDb.SharedGame.Zones.WorldData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GroundObjectLoader : BaseCrawlerMapObjectLoader
{
    public override long HelperKey => EntityTypes.GroundObject;
    protected override string GetLayerName() { return LayerNames.ObjectLayer; }

    public override async Awaitable Load(OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        GroundObjType groundObjType = _gameData.Get<GroundObjTypeSettings>(_gs.ch).Get(spawn.EntityId);
        if (groundObjType == null)
        {
            return;
        }
        float wx = spawn.X;
        float wz = spawn.Z;

        SpawnLoadData loadData = new SpawnLoadData()
        {
            Obj = obj,
            Spawn = spawn,
            Token = token,
        };

        _assetService.LoadAsset(AssetCategoryNames.Props, groundObjType.Art, OnDownloadGroundObject, null, token, loadData);


        await Task.CompletedTask;
        return;
    }

    private void OnDownloadGroundObject(GameObject go, SpawnLoadData loadData, CancellationToken token)
    {
        MapGroundObject worldGroundObject = _clientEntityService.GetOrAddComponent<MapGroundObject>(go);

        GroundObjType gtype = _gameData.Get<GroundObjTypeSettings>(_gs.ch).Get(loadData.Spawn.EntityId);

        worldGroundObject.GroundObjectId = gtype.IdKey;
        worldGroundObject.CrafterTypeId = gtype.CrafterTypeId;
        worldGroundObject.Init(loadData.Obj, go, token);
        worldGroundObject.GroundObj = gtype;
        worldGroundObject.X = (int)loadData.Spawn.X;
        worldGroundObject.Z = (int)loadData.Spawn.Z;
        if (loadData.Spawn.ZoneId > 0)
        {
            Zone zone = _mapProvider.GetMap().Get<Zone>(loadData.Spawn.ZoneId);
            if (zone != null)
            {
                worldGroundObject.Level = zone.Level;
            }
        }

        if (gtype.CrafterTypeId > 0 && gtype.SpawnTableId > 0)
        {
            worldGroundObject.ShowGlow(0);
        }
        FinalPlaceObject(go, loadData, LayerNames.ObjectLayer);
    }
}






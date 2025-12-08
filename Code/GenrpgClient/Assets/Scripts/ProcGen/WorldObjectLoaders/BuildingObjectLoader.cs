
using Assets.Scripts.Buildings;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingObjectLoader : BaseMapObjectLoader
{
    public override long HelperKey => EntityTypes.Building;
    protected override string GetLayerName() { return LayerNames.ObjectLayer; }

    public override async Awaitable Load(OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        BuildingType buildingType = _gameData.Get<BuildingSettings>(_gs.ch).Get(spawn.EntityId);
        if (buildingType == null)
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

        _assetService.LoadAsset(AssetCategoryNames.Buildings, "Default/" + buildingType.Art, OnDownloadBuildingObject, null, token, loadData);


        await Task.CompletedTask;
        return;
    }

    private void OnDownloadBuildingObject(GameObject go, SpawnLoadData loadData, CancellationToken token)
    {
        loadData.FixedPosition = true;
        MapBuilding building = _clientEntityService.GetOrAddComponent<MapBuilding>(go);

        BuildingType buildingType = _gameData.Get<BuildingSettings>(_gs.ch).Get(loadData.Spawn.EntityId);

        building.Init(buildingType, loadData.Spawn);

        FinalPlaceObject(go, loadData, LayerNames.ObjectLayer);
    }
}




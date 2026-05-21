using Assets.Scripts.Assets.Constants;
using Assets.Scripts.MapTerrain;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Combat.Messages;
using OxDb.SharedGame.Constants;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using OxDb.SharedGame.Units.Constants;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Units.Settings;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UnitObjectLoader : BaseMapObjectLoader
{
    public override long HelperKey => EntityTypes.Unit;
    protected override string GetLayerName() { return LayerNames.UnitLayer; }

    public override async Awaitable Load(OnSpawn spawn, MapObject obj, CancellationToken token)
    {

        UnitType utype = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(spawn.EntityId);
        if (utype == null)
        {
            return;
        }

        SpawnLoadData loadData = new SpawnLoadData()
        {
            Spawn = spawn,
            Obj = obj,
            Token = token,
        };


        _assetService.LoadAsset(AssetCategoryNames.Monsters, utype.Art, AfterLoadUnit, null, token, loadData);
        await Task.CompletedTask;
    }



    private IUnitSetupService _zoneGenService = null;
    protected virtual void AfterLoadUnit(GameObject artGo, SpawnLoadData loadData, CancellationToken token)
    {
        if (_objectManager.GetController(loadData.Spawn.ObjId, out UnitController currController))
        {
            _clientEntityService.Destroy(artGo);
            return;
        }

        GameObject go = _zoneGenService.SetupUnit(artGo, loadData, loadData.Token);
        float height = _terrainManager.SampleHeight(loadData.Obj.X, loadData.Obj.Z);

        TerrainPatchData patch = _terrainManager.GetPatchFromMapPos(loadData.Obj.X, loadData.Obj.Z);

        Vector3 oldScale = go.transform.localScale;
        if (patch != null)
        {
            Terrain terr = patch.terrain as Terrain;

            if (terr != null)
            {
                _clientEntityService.AddToParent(go, terr.gameObject);
            }
            else
            {
                _clientEntityService.AddToParent(go, _terrainManager.GetPrototypeParent());
            }
        }

        go.transform.position = new Vector3(loadData.Obj.X, height, loadData.Obj.Z);
        go.transform.eulerAngles = new Vector3(0, loadData.Obj.Rot, 0);
        go.transform.localScale = oldScale;
        if (loadData.Obj is Unit unit)
        {
            if (unit.HasFlag(UnitFlags.IsDead))
            {
                UnitController unitController = go.GetComponent<UnitController>();
                if (unitController != null)
                {
                    unitController.OnDeath(new Died(), loadData.Token);
                }
            }
        }

        if (height == 0)
        {
            _awaitableService.ForgetAwaitable(WaitForTerrain(go, loadData, loadData.Token));
        }

        _objectManager.AddObject(loadData.Obj, go);

    }

    private async Awaitable WaitForTerrain(GameObject go, SpawnLoadData loadData, CancellationToken token)
    {
        int times = 0;
        while (!token.IsCancellationRequested && ++times < 1000)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
            float height = _terrainManager.SampleHeight(loadData.Obj.X, loadData.Obj.Z);
            if (height > 0)
            {
                go.transform.position = new Vector3(loadData.Obj.X, height, loadData.Obj.Z);
                break;
            }
        }
    }
}



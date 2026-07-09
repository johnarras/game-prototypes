
using Assets.Scripts.Assets.Constants;
using Assets.Scripts.GameObjects;
using OxDb.SharedGame.Constants;
using System.Threading;
using UnityEngine;

public class AddMinGroundLevel : BaseZoneGenerator
{
    private ISingletonContainer _singletonContainer = null;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        AddKillCollider(_gs);
    }

    private GameObject _killCollider = null;
    public void AddKillCollider(IClientGameState gs)
    {
        if (_killCollider != null)
        {
            return;
        }

        _assetService.LoadAssetInto(_singletonContainer.GetAssetParent<KillCollider>(),
            AssetCategoryNames.Prefabs, MapConstants.KillColliderName, OnLoadKillCollider, _token, default(object));

        _assetService.LoadAsset<object>(AssetCategoryNames.Prefabs, MapConstants.WaterName, OnLoadBigWater, default(object), _token);
    }

    private void OnLoadKillCollider(GameObject go, object data, CancellationToken token)
    {
        _killCollider = go;
    }

    private void OnLoadBigWater(GameObject go, object data, CancellationToken token)
    {
        go.transform.position = new Vector3(0, MapConstants.MinLandHeight - 5, 0);
        go.transform.localScale = new Vector3(100000, 1, 100000);
        _clientEntityService.SetLayer(go, LayerUtils.NameToLayer(LayerNames.Water));
    }
}





using Assets.Scripts.GameObjects;
using Genrpg.Shared.Client.Assets.Constants;
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
    }

    private void OnLoadKillCollider(GameObject go, object data, CancellationToken token)
    {
        _killCollider = go;
    }
}




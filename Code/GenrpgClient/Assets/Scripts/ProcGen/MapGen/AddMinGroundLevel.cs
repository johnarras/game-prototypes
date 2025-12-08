
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Core;
using System.Threading;
using UnityEngine;

// Settings for YES reflection
// Fres Int: 0.1, Pow: 1.08 Bias: 0

// Settings for NO reflection
// Fres: Int 0.1, Pow: 3.5, Bias: -3.0


public class AddMinGroundLevel : BaseZoneGenerator
{
    private IInitClient _initClient;
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
        _assetService.LoadAssetInto(_initClient.GetRootObject(), AssetCategoryNames.Prefabs, MapConstants.KillColliderName, OnLoadKillCollider, _token, default(object));
    }

    private void OnLoadKillCollider(GameObject go, object data, CancellationToken token)
    {
        _killCollider = go;
    }

}


using Assets.Scripts.Assets;
using Assets.Scripts.GameObjects;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spells.Messages;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class FullFX
{
    public FX fx;
    public MapObject from;
    public MapObject to;
    public GameObject fromObj;
    public GameObject toObj;
    public CancellationToken token;

}

public interface IFxService : IInitializable
{
    void ShowFX(FX fx, CancellationToken token);
}

public class FxService : IFxService
{
    private IClientMapObjectManager _objectManager;
    private IAssetService _assetService = null;
    private IClientGameState _gs;
    protected IClientEntityService _clientEntityService = null;

    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }


    public void ShowFX(FX fx, CancellationToken token)
    {
        if (!_objectManager.GetGridItem(fx.From, out ClientMapObjectGridItem from))
        {
            return;
        }
        if (!_objectManager.GetGridItem(fx.To, out ClientMapObjectGridItem to))
        {
            return;
        }

        FullFX full = new FullFX()
        {
            from = from.Obj,
            to = to.Obj,
            fx = fx,
            fromObj = from?.Controller?.gameObject,
            toObj = to?.Controller?.gameObject,
            token = token,
        };

        if (full.fromObj == null || full.toObj == null)
        {
            return;
        }

        _assetService.LoadAsset(AssetCategoryNames.Magic, fx.Art, OnLoadFX, null, token, full);
    }

    private void OnLoadFX(GameObject go, FullFX full, CancellationToken token)
    {
        if (full == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }
        MapProjectile proj = _clientEntityService.GetOrAddComponent<MapProjectile>(go);

        proj.Init(full, token);

    }
}


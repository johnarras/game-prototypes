using Assets.Scripts.Assets.Constants;
using Assets.Scripts.GameObjects;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.UI.TraderHUD
{
    public class TraderHUDScreen : BaseScreen
    {
        private ISingletonContainer _singletonContainer = null;
        protected override async Task OnStartOpen(object data, CancellationToken token)
        {


            _assetService.LoadAssetInto(_singletonContainer.GetSingleton("TraderTerrainParent"),
                AssetCategoryNames.Biomes, "TraderTerrain", OnLoadTerrain, token, data);

            await Task.CompletedTask;
        }

        private void OnLoadTerrain(GameObject go, object data, CancellationToken token)
        {
        }
    }
}



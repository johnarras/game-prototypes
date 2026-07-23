using OxDb.Client.Assets.Constants;
using OxDb.Client.Core.Interfaces;
using OxDb.Client.GameObjects;
using OxDb.SharedCore.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Trader.WorldMap.Services
{

    public interface ITraderTerrainService : IInitializable, IClientResetCleanup
    {
        void ShowTerrain();
        void HideTerrain();
    }

    public class TraderTerrainService : ITraderTerrainService
    {
        private ISingletonContainer _singletonContainer = null;
        private IAssetService _assetService = null;
        private IClientEntityService _clientEntityService = null;


        private TraderTerrain _terrain = null;

        string TraderTerrainParentName = "TraderTerrainParent";
        private CancellationToken _token;

        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            await Task.CompletedTask;
        }

        public void HideTerrain()
        {

            _clientEntityService.Destroy(_terrain);
            _terrain = null;
        }

        public async Task OnReset(CancellationToken token)
        {
            HideTerrain();
            await Task.CompletedTask;
        }

        public void ShowTerrain()
        {

            _assetService.LoadAsset<object>(
                AssetCategoryNames.Biomes, "TraderTerrain", OnLoadTerrain, _singletonContainer.GetSingleton(TraderTerrainParentName),
                _token);
        }



        private void OnLoadTerrain(GameObject go, object data, CancellationToken token)
        {
            _terrain = go.GetComponent<TraderTerrain>();
        }
    }
}

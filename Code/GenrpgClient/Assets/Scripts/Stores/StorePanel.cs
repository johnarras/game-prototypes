using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Awaitables;
using Assets.Scripts.UI.Stores;
using OxDb.SharedGame.Purchasing.PlayerData;
using OxDb.SharedGame.Purchasing.Settings;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Stores
{
    public class StorePanel : BaseBehaviour
    {

        protected IAwaitableService _awaitableService = null;

        public long StoreSlotId;

        public StoreArtPanel ArtPanel;

        public GameObject ArtAnchor;

        protected StoreScreen _screen;
        protected PlayerStoreOffer _offer;
        protected StoreTheme _theme;

        public async Task Init(StoreScreen screen, PlayerStoreOffer offer, CancellationToken token)
        {

            if (ArtAnchor == null)
            {
                return;
            }

            _screen = screen;
            _offer = offer;

            _theme = _gameData.Get<StoreThemeSettings>(_gs.ch).Get(offer.StoreThemeId);

            if (_theme == null)
            {
                _theme = _gameData.Get<StoreThemeSettings>(_gs.ch).GetData().First();
            }

            if (ArtPanel != null && ArtPanel.ThemeId == _theme.IdKey)
            {
                await ArtPanel.Init(screen, offer, _theme, token);
                return;
            }
            else
            {
                _clientEntityService.DestroyAllChildren(ArtAnchor);
                _assetService.LoadAssetInto<object>(ArtAnchor, AssetCategoryNames.Stores, _theme.Art + "StoreArtPanel", OnDownloadStoreArt, GetToken());
            }
        }

        private void OnDownloadStoreArt(GameObject go, object data, CancellationToken token)
        {
            StoreArtPanel artPanel = go.GetComponent<StoreArtPanel>();

            if (artPanel == null)
            {
                _clientEntityService.Destroy(go);
                _clientEntityService.SetActive(gameObject, false);
                return;
            }

            _awaitableService.ForgetTask(artPanel.Init(_screen, _offer, _theme, GetToken()));
        }
    }
}




using Assets.Scripts.Awaitables;
using OxDb.SharedGame.Purchasing.PlayerData;
using OxDb.SharedGame.Purchasing.Settings;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Stores
{
    public class StoreArtPanel : BaseBehaviour
    {
        protected IAwaitableService _awaitableService = null;

        public GText Header;
        public GameObject BundleParent;

        public StoreBundlePanel BundlePanelPrefab;
        public StoreRewardPanel RewardPanelPrefab;

        public List<StoreBundlePanel> Panels = new List<StoreBundlePanel>();

        protected StoreScreen _screen;
        protected PlayerStoreOffer _offer;
        protected StoreTheme _theme;

        public long Index => _offer?.StoreSlotId ?? 0;

        public long ThemeId => _theme?.IdKey ?? 0;

        public async Task Init(StoreScreen screen, PlayerStoreOffer offer, StoreTheme theme, CancellationToken token)
        {
            _screen = screen;
            _offer = offer;

            _theme = theme;

            if (BundleParent == null)
            {
                return;
            }

            _uiService.SetText(Header, _offer.Name);

            List<Task> initTasks = new List<Task>();

            for (int b = 0; b < offer.Bundles.Count; b++)
            {
                if (b >= Panels.Count)
                {
                    StoreBundlePanel newPanel = _clientEntityService.FullInstantiate<StoreBundlePanel>(BundlePanelPrefab);
                    _clientEntityService.AddToParent(newPanel.gameObject, BundleParent);
                    Panels.Add(newPanel);
                }
                initTasks.Add(Panels[b].Init(offer, offer.Bundles[b], _screen.GetName(), RewardPanelPrefab, token));
            }

            for (int b = offer.Bundles.Count; b < Panels.Count; b++)
            {
                _clientEntityService.SetActive(Panels[b], false);
            }

            await Task.WhenAll(initTasks);
        }
    }
}



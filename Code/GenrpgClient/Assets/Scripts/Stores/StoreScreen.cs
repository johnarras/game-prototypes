using Assets.Scripts.Stores;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Purchasing.WebApi.RefreshStores;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Stores
{
    public class StoreScreen : BaseScreen
    {

        public GameObject StoreParent;

        public List<StorePanel> Panels = new List<StorePanel>();

        private bool _didPassInOffer = false;
        private List<PlayerStoreOffer> _offers = new List<PlayerStoreOffer>();
        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            if (data is PlayerStoreOffer offer)
            {
                _didPassInOffer = true;
                _offers.Add(offer);
            }
            else
            {
                _offers = GetOfferList();
            }

            AddListener<RefreshStoresResponse>(OnRefreshStores);

            await SetupData(GetToken());

        }

        protected List<PlayerStoreOffer> GetOfferList()
        {

            List<PlayerStoreOffer> customOffers = _gs.ch.Get<PlayerStoreOfferData>().StoreOffers.ToList();

            List<PlayerStoreOffer> defaultOffers = _gameData.Get<DefaultStoreOfferSettings>(_gs.ch).Offers.ToList();

            List<PlayerStoreOffer> finalList = defaultOffers;

            foreach (PlayerStoreOffer offer in customOffers)
            {
                PlayerStoreOffer defaultOffer = finalList.FirstOrDefault(x => x.StoreSlotId == offer.StoreSlotId);

                if (defaultOffer != null)
                {
                    finalList.Remove(defaultOffer);
                }

                finalList.Add(offer);
            }

            finalList = finalList.OrderBy(x => x.StoreSlotId).ToList();

            return finalList;
        }

        private async Task SetupData(CancellationToken token)
        {
            if (_offers.Count < 1 || StoreParent == null)
            {
                StartClose();
                return;
            }

            List<Task> initTasks = new List<Task>();


            foreach (PlayerStoreOffer offer in _offers)
            {

                // If did not pass in offer, we need a panel for that store slot or don't show.

                // If did not pass in offer, need existing panel to show this store.
                StorePanel panel = Panels.FirstOrDefault(x => x.StoreSlotId == offer.StoreSlotId);

                if (panel == null)
                {
                    continue;
                }

                _clientEntityService.SetActive(panel, true);
                initTasks.Add(panel.Init(this, offer, token));
            }

            foreach (StorePanel panel in Panels)
            {
                if (!_offers.Any(x => x.StoreSlotId == panel.StoreSlotId))
                {
                    _clientEntityService.SetActive(panel, false);
                }
            }

            await Task.WhenAll(initTasks);
        }

        private void OnRefreshStores(RefreshStoresResponse result)
        {
            if (!_didPassInOffer)
            {
                _offers = GetOfferList();
            }
            else
            {
                if (_offers.Count > 0)
                {
                    PlayerStoreOffer newOffer = GetOfferList().FirstOrDefault(x => x.StoreSlotId == _offers[0].StoreSlotId);
                    if (newOffer != null)
                    {
                        _offers = new List<PlayerStoreOffer> { newOffer };
                    }
                }
            }

            _awaitableService.ForgetTask(SetupData(GetToken()));
            return;
        }
    }
}

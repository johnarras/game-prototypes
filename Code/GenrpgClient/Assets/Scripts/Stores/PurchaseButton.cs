using Assets.Scripts.Purchasing.Services;
using OxDb.SharedGame.Purchasing.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Stores
{
    public class PurchaseButton : BaseBehaviour
    {

        protected PlayerBundle _bundle = null;
        protected PlayerStoreOffer _offer = null;
        protected IClientPurchasingService _purchasingService = null;
        public GButton Button;
        public async Awaitable Init(PlayerStoreOffer offer, PlayerBundle bundle)
        {

            _bundle = bundle;
            _offer = offer;
            _uiService.SetButton(Button, GetName(), ClickPurchaseButton);
            await Task.CompletedTask;
        }

        private async ValueTask ClickPurchaseButton(CancellationToken token)
        {

            await _purchasingService.PurchaseBundle(_offer, _bundle, token);

            await Task.CompletedTask;
        }
    }
}



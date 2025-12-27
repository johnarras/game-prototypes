using Assets.Scripts.ClientEvents.UI;
using Genrpg.Shared.PlayMultiplier.WebApi;
using Genrpg.Shared.UI.Constants;

namespace Assets.Scripts.Trader.UI.TraderHUD
{
    public class TraderHUDButtons : BaseBehaviour
    {

        protected IClientWebService _webService = null;
        protected IScreenService _screenService = null;

        public GButton StoreButton;
        public GButton MultButton;
        public override void Init()
        {
            _uiService.SetButton(StoreButton, GetName(), ClickStoreButton);
            _uiService.SetButton(MultButton, GetName(), ClickMultButton);
        }

        private void ClickStoreButton()
        {
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.Store));
        }

        private void ClickMultButton()
        {
            _webService.SendClientUserWebRequest(new SetPlayMultRequest() { PlayMult = 2 }, GetToken());
        }
    }
}



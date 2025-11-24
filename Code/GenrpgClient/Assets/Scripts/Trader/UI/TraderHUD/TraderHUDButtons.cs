using Genrpg.Shared.PlayMultiplier.WebApi;
using Genrpg.Shared.UI.Constants;

namespace Assets.Scripts.Trader.UI.TraderHUD
{
    public class TraderHUDButtons : BaseBehaviour
    {

        protected IClientWebService _webService = null;

        public GButton StoreButton;
        public GButton MultButton;
        public override void Init()
        {
            _uiService.SetButton(StoreButton, GetName(), ClickStoreButton);
            _uiService.SetButton(MultButton, GetName(), ClickMultButton);
        }

        private void ClickStoreButton()
        {
            _screenService.Open(ScreenNames.Store);
        }

        private void ClickMultButton()
        {
            _webService.SendClientUserWebRequest(new SetPlayMultRequest() { PlayMult = 2 }, GetToken());
        }
    }
}

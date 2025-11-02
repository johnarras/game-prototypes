using Genrpg.Shared.UI.Constants;

namespace Assets.Scripts.Trader.UI.TraderHUD
{
    public class TraderHUDButtons : BaseBehaviour
    {

        public GButton StoreButton;

        public override void Init()
        {
            _uiService.SetButton(StoreButton, GetName(), ClickStoreButton);

        }

        private void ClickStoreButton()
        {
            _screenService.Open(ScreenNames.Store);
        }
    }
}

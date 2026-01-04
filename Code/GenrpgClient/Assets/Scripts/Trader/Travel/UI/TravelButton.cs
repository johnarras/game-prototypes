using Assets.Scripts.Trader.Travel.Services;

namespace Assets.Scripts.Trader.UI.Travel
{
    public class TravelButton : BaseBehaviour
    {

        private IClientTravelService _clientTravelService = null;
        public GButton Button;


        public override void Init()
        {

            _uiService.SetButton(Button, GetName(), ClickTravelButton);

        }

        private void ClickTravelButton()
        {
            _clientTravelService.ClickTravelButton();
        }
    }
}

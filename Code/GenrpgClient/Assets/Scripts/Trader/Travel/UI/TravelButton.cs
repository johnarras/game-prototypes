using OxDb.Client.Trader.Travel.Services;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Trader.UI.Travel
{
    public class TravelButton : BaseBehaviour
    {

        private IClientTravelService _clientTravelService = null;
        public GButton Button;


        public override void Init()
        {

            _uiService.SetButton(Button, GetName(), ClickTravelButton);

        }

        private async ValueTask ClickTravelButton(CancellationToken token)
        {
            await _clientTravelService.ClickTravelButton(token);
        }
    }
}

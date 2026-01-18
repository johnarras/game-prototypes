using Assets.Scripts.ClientEvents.UI;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.Trader.Camping.WebApi;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.UI.Constants;
using System.Collections.Generic;

namespace Assets.Scripts.Trader.UI.TraderHUD
{
    public class TraderHUDButtons : BaseBehaviour
    {

        protected IClientWebService _webService = null;
        protected IScreenService _screenService = null;
        protected ICaravanService _caravanService = null;

        public GButton AnimalsButton;
        public GButton TradeGoodsButton;
        public GButton CampButton;
        public GButton ChangeHeadingButton;
        public GButton MapButton;

        public override void Init()
        {
            _uiService.SetButton(AnimalsButton, GetName(), () => { OpenScreenNamed(ScreenNames.Animals); });
            _uiService.SetButton(TradeGoodsButton, GetName(), () => { OpenScreenNamed(ScreenNames.TradeGoods); });
            _uiService.SetButton(CampButton, GetName(), ClickCamp);
            _uiService.SetButton(ChangeHeadingButton, GetName(), ClickChangeHeading);
            _uiService.SetButton(MapButton, GetName(), ClickMapButton);
        }

        private void OpenScreenNamed(long screenName)
        {
            _dispatcher.Dispatch(new OpenScreen(screenName));
        }

        private void ClickCamp()
        {
            CoreData coreData = _gs.ch.Get<CoreData>();

            TraderStatData statData = _gs.ch.Get<TraderStatData>();

            IReadOnlyList<CoreCurrencyType> ctypes = _gameData.Get<CoreCurrencyTypeSettings>(_gs.ch).GetData();

            _webService.SendClientUserWebRequest(new CampRequest(), GetToken());

        }

        private void ClickChangeHeading()
        {
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.TraderCityRoads));
        }


        private void ClickMapButton()
        {
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.TraderMap));
        }

    }
}



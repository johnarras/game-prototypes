using Assets.Scripts.ClientEvents.UI;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.Trader.Camping.WebApi;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Roads.WebApi;
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
        public GButton TurnAroundButton;

        public override void Init()
        {
            _uiService.SetButton(AnimalsButton, GetName(), () => { OpenScreenNamed(ScreenNames.Animals); });
            _uiService.SetButton(TradeGoodsButton, GetName(), () => { OpenScreenNamed(ScreenNames.TradeGoods); });
            _uiService.SetButton(CampButton, GetName(), ClickCamp);
            _uiService.SetButton(TurnAroundButton, GetName(), ClickTurnAround);
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

        private void ClickTurnAround()
        {
            CaravanPosition position = _caravanService.GetPosition(_gs.ch.Get<CoreData>());

            if (position.CityId > 0)
            {
                _dispatcher.Dispatch(new ShowFloatingText("You are in a city!", EFloatingTextArt.Error));
            }

            _webService.SendClientUserWebRequest(new TurnAroundRequest(), GetToken());
        }


    }
}



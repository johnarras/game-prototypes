using Assets.Scripts.ClientEvents.UI;
using OxDb.SharedGame.Trader.Cities.Settings;
using OxDb.SharedGame.UI.Constants;

namespace Assets.Scripts.Trader.Cities.UI
{
    public class TraderCityPanel : BaseBehaviour
    {
        public GButton CaravanMembersButton;
        public GButton TradeGoodsButton;
        public GButton GuildButton;
        public GButton QuestsButton;
        public GButton SuppliesButton;
        public GButton EnchanterButton;
        public GButton TempleButton;
        public GButton TrainerButton;
        public GButton TravelButton;

        private City _city = null;
        public void SetData(City city)
        {
            _city = city;

            _uiService.SetButton(CaravanMembersButton, GetName(), () => { OpenScreenNamed(ScreenNames.TraderCaravanMembers); });
            _uiService.SetButton(TradeGoodsButton, GetName(), () => { OpenScreenNamed(ScreenNames.TradeGoods); });
            _uiService.SetButton(GuildButton, GetName(), () => { OpenScreenNamed(ScreenNames.TraderQuests); });
            _uiService.SetButton(QuestsButton, GetName(), () => { OpenScreenNamed(ScreenNames.TraderQuests); });
            _uiService.SetButton(SuppliesButton, GetName(), () => { OpenScreenNamed(ScreenNames.Supplies); });
            _uiService.SetButton(EnchanterButton, GetName(), () => { OpenScreenNamed(ScreenNames.Enchanter); });
            _uiService.SetButton(TempleButton, GetName(), () => { OpenScreenNamed(ScreenNames.TraderTemple); });
            _uiService.SetButton(TrainerButton, GetName(), () => { OpenScreenNamed(ScreenNames.TraderTrainer); });
            _uiService.SetButton(TravelButton, GetName(), () => { OpenScreenNamed(ScreenNames.TraderCityRoads); });

        }
        private void OpenScreenNamed(long screenName)
        {
            _dispatcher.Dispatch(new OpenScreen(screenName));
        }

    }
}

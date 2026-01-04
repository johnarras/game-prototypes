using Assets.Scripts.Trader.UI.TradeMapUI;
using Genrpg.Shared.Trader.Cities.Settings;

namespace Assets.Scripts.Trader.UI.TraderMapUI
{
    public class TraderMapCityButton : BaseBehaviour
    {
        public GButton Button;
        public GText CityName;


        private TraderMapScreen _screen = null;
        private City _city = null;
        public void InitCity(TraderMapScreen screen, City city)
        {
            _city = city;
            _screen = screen;
            _uiService.SetButton(Button, "CityIcon" + _city.Icon, OnClickCity);
            _uiService.SetText(CityName, _city.Name);
            name = _city.Name;
        }

        public City GetCity()
        {
            return _city;
        }

        private void OnClickCity()
        {
            _logService.Info("Clicked " + _city?.Name ?? "No City");
            _screen.ClickCityUI(_city.IdKey);
        }
    }
}



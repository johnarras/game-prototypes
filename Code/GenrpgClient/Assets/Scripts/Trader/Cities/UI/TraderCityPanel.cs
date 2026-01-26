using Genrpg.Shared.Trader.Cities.Settings;

namespace Assets.Scripts.Trader.Cities.UI
{
    public class TraderCityPanel : BaseBehaviour
    {
        public GButton AnimalsButton;
        public GButton VendorButton;
        public GButton GuildButton;
        public GButton QuestsButton;
        public GButton SuppliesButton;

        private City _city = null;
        public void SetData(City city)
        {
            _city = city;



        }
    }
}

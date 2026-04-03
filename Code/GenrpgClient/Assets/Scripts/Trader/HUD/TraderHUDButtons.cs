using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Doobers.Events;
using Genrpg.Shared.Attributes.PlayerData;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Camping.WebApi;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.UI.Constants;
using System.Collections.Generic;

namespace Assets.Scripts.Trader.UI.TraderHUD
{
    public class TraderHUDButtons : BaseBehaviour
    {

        protected IClientWebService _webService = null;
        protected IScreenService _screenService = null;
        protected ICaravanService _caravanService = null;

        public GButton CaravanButton;
        public GButton TradeGoodsButton;
        public GButton CampButton;
        public GButton ChangeHeadingButton;
        public GButton MapButton;
        public GButton SpellbookButton;
        public GButton RepairButton;
        public GButton EstateButton;
        public GButton ManifestButton;

        public override void Init()
        {
            _uiService.SetButton(CaravanButton, GetName(), () => { OpenScreenNamed(ScreenNames.Caravan); });
            _uiService.SetButton(TradeGoodsButton, GetName(), () => { OpenScreenNamed(ScreenNames.TradeGoods); });
            _uiService.SetButton(CampButton, GetName(), ClickCamp);
            _uiService.SetButton(ChangeHeadingButton, GetName(), ClickChangeHeading);
            _uiService.SetButton(MapButton, GetName(), ClickMapButton);
            _uiService.SetButton(SpellbookButton, GetName(), () => { OpenScreenNamed(ScreenNames.TraderSpells); });
            _uiService.SetButton(RepairButton, GetName(), () => { OpenScreenNamed(ScreenNames.Repair); });
            _uiService.SetButton(ManifestButton, GetName(), () => { OpenScreenNamed(ScreenNames.Manifest); });
            _uiService.SetButton(EstateButton, GetName(), () => { OpenScreenNamed(ScreenNames.Estate); });

            _dispatcher.Dispatch(new SetDooberTarget(EntityTypes.TradeGood, 0, TradeGoodsButton.gameObject, true, null));
            _dispatcher.Dispatch(new SetDooberTarget(EntityTypes.CaravanMember, 0, CaravanButton.gameObject, true, null));
            

            _dispatcher.Dispatch(new SetDooberTarget(EntityTypes.CoreCurrency, CoreCurrencyTypes.Stone, EstateButton.gameObject, true, null));
            _dispatcher.Dispatch(new SetDooberTarget(EntityTypes.CoreCurrency, CoreCurrencyTypes.Wood, EstateButton.gameObject, true, null));
            _dispatcher.Dispatch(new SetDooberTarget(EntityTypes.CoreCurrency, CoreCurrencyTypes.Metal, EstateButton.gameObject, true, null));
            _dispatcher.Dispatch(new SetDooberTarget(EntityTypes.CoreCurrency, CoreCurrencyTypes.Leather, EstateButton.gameObject, true, null));
            _dispatcher.Dispatch(new SetDooberTarget(EntityTypes.CoreCurrency, CoreCurrencyTypes.Gems, EstateButton.gameObject, true, null));
        }

        private void OpenScreenNamed(long screenName)
        {
            _dispatcher.Dispatch(new OpenScreen(screenName));
        }

        private void ClickCamp()
        {
            CoreData coreData = _gs.ch.Get<CoreData>();

            AttributeData attributeData = _gs.ch.Get<AttributeData>();

            IReadOnlyList<CoreCurrencyType> ctypes = _gameData.Get<CoreCurrencyTypeSettings>(_gs.ch).GetData();

            _webService.SendWebRequest(new CampRequest(), GetToken());

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



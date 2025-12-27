using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Roads.Settings;
using Genrpg.Shared.Trader.Roads.WebApi;

namespace Assets.Scripts.Trader.UI.Cities
{

    public class TraderRoadArgs
    {
        public Road Road { get; set; }
        public bool CanTravel { get; set; }
        public long FromCityId { get; set; }
    }

    public class TraderRoadRowUI : BaseBehaviour
    {

        private IClientWebService _webService = null;
        public GText InfoText;
        public GButton Button;

        private TraderRoadArgs _args;
        public void SetData(TraderRoadArgs args)
        {
            _args = args;

            long otherCityId = args.Road.GetCityIdOnOtherEnd(args.FromCityId);

            City otherCity = _gameData.Get<CitySettings>(_gs.ch).Get(otherCityId);

            _uiService.SetText(InfoText, args.Road.Name + " toward " + otherCity.Name + ": (" + args.Road.Distance + " " + TraderConstants.DistanceAbbreviation + ")");

            _uiService.SetButton(Button, GetName(), OnClickButton);
        }

        private void OnClickButton()
        {
            _webService.SendClientUserWebRequest(new EnterRoadRequest() { RoadId = _args.Road.IdKey }, GetToken());
        }
    }
}

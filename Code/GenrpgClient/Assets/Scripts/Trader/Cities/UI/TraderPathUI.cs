using OxDb.Client.Networking.Services;
using OxDb.SharedGame.Trader.Cities.Settings;
using OxDb.SharedGame.Trader.Constants;
using OxDb.SharedGame.Trader.Travel.WebApi;
using System.Text;
using UnityEngine;

namespace OxDb.Client.Trader.UI.Cities
{

    public class TraderRoadArgs
    {
        public City TargetCity { get; set; }
        public int TargetX { get; set; }
        public int TargetY { get; set; }
        public int DistanceToTarget { get; set; }
        public float Angle { get; set; }
    }

    public class TraderPathUI : BaseBehaviour
    {

        private IClientWebRequestService _webService = null;
        public GText InfoText;
        public GButton Button;
        public GImage CompassImage;

        private TraderRoadArgs _args;
        public void SetData(TraderRoadArgs args)
        {
            _args = args;

            StringBuilder sb = new StringBuilder();
            sb.Append("To: " + args.TargetCity != null ? args.TargetCity.Name : "The Wilderness");
            sb.Append(" (" + args.DistanceToTarget + " " + TraderConstants.DistanceAbbreviation + ")");

            _uiService.SetText(InfoText, sb.ToString());

            _uiService.SetButton(Button, GetName(), OnClickButton);

            if (CompassImage != null)
            {
                CompassImage.transform.eulerAngles = new Vector3(0, 0, -args.Angle);
            }
        }

        private void OnClickButton()
        {
            _webService.SendMainServerRequest(new HeadToTargetRequest() { ToX = _args.TargetX, ToZ = _args.TargetY, ToCityId = (int)(_args.TargetCity?.IdKey ?? 0) }, GetToken());
        }
    }
}

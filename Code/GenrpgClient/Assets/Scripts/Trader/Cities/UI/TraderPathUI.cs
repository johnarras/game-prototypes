using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Travel.WebApi;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Trader.UI.Cities
{

    public class TraderRoadArgs
    {
        public City TargetCity { get; set; }
        public long TargetX { get; set; }
        public long TargetY { get; set; }
        public long DistanceToTarget { get; set; }
        public float Angle { get; set; }
    }

    public class TraderPathUI : BaseBehaviour
    {

        private IClientWebService _webService = null;
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
            _webService.SendClientUserWebRequest(new HeadToTargetRequest() { ToX = _args.TargetX, ToY = _args.TargetY, ToCityId = _args.TargetCity?.IdKey ?? 0 }, GetToken());
        }
    }
}

using Assets.Scripts.Trader.Travel.ClientEvents;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;

namespace Assets.Scripts.Trader.Travel.UI
{
    public class TravelInfoUI : BaseBehaviour
    {

        private ICaravanService _caravanService = null;

        public GText DistanceText;
        public GText CostPerDayText;
        public GText TotalCostText;


        public override void Init()
        {
            _dispatcher.AddListener<OnUpdateVisualPlayMult>(UpdatedVisualPlayMult, GetToken());
            base.Init();
            ShowData();
        }


        private void UpdatedVisualPlayMult(OnUpdateVisualPlayMult response)
        {
            ShowData();
        }

        private void ShowData()
        {
            CoreData coreData = _gs.ch.Get<CoreData>();
            CaravanTravelInfo info = _caravanService.GetTravelInfo(coreData);



            _uiService.SetText(DistanceText, "Speed/Day: " + info.DiceDistancePerDay + " <sprite name=\"Die5\">" +
                (info.BonusDistancePerDay > 0 ? " + " + info.BonusDistancePerDay : ""));

            _uiService.SetText(CostPerDayText, "Cost/Day: <sprite name=\"Food\"> " + info.CostPerDay);

            _uiService.SetText(TotalCostText, "Total Cost: <sprite name=\"Food\"> " + info.TotalCost);

        }
    }
}

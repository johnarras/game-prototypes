using Assets.Scripts.Trader.Travel.ClientEvents;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.CoreCurrencies.Settings;
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
            _dispatcher.AddListener<VisualUpdateTravelStats>(OnUpdateVisualTravelStats, GetToken());
            base.Init();
            ShowData();
        }


        private void OnUpdateVisualTravelStats(VisualUpdateTravelStats response)
        {
            ShowData();
        }


        string rationsSpriteString = null;
        private void ShowData()
        {
            CoreData coreData = _gs.ch.Get<CoreData>();
            CaravanTravelInfo info = _caravanService.GetTravelInfo(coreData);


            if (string.IsNullOrEmpty(rationsSpriteString))
            {
                rationsSpriteString = "<sprite name=\"" + _gameData.Get<CoreCurrencyTypeSettings>(_gs.ch).Get(CoreCurrencyTypes.Rations).Name + "\"> ";
            }

            _uiService.SetText(DistanceText, info.DiceSpeed + " <sprite name=\"Die5\">" +
                (info.BonusSpeed > 0 ? " + " + info.BonusSpeed : ""));

            _uiService.SetText(CostPerDayText, info.CostPerDay.ToString());

            _uiService.SetText(TotalCostText, info.TotalCost.ToString());

        }
    }
}

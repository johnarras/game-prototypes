using OxDb.Client.Trader.ClientEvents;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Trader.Calendar.Services;
using OxDb.SharedGame.Trader.Constants;

namespace OxDb.Client.Trader.UI.TraderHUD
{
    public class GameplayStatusUI : BaseBehaviour
    {
        private ICalendarService _calendarService = null;

        public GText DateText;

        public override void Init()
        {
            base.Init();

            _dispatcher.AddListener<UpdateTraderHUD>(OnUpdateTraderHUD, GetToken());


            ShowData();

        }

        private void OnUpdateTraderHUD(UpdateTraderHUD update)
        {
            ShowData();
        }

        private void ShowData()
        {
            CoreData coreData = _gs.ch.Get<CoreData>();

            if (coreData != null)
            {
                _uiService.SetText(DateText, _calendarService.PrintDay(coreData.Vars[TraderVars.PlayCount]));
            }
        }
    }
}

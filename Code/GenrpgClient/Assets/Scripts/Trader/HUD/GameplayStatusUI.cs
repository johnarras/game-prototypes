using Assets.Scripts.Trader.ClientEvents;
using Assets.Scripts.Trader.Info.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Calendar.Services;
using Genrpg.Shared.Trader.Constants;

namespace Assets.Scripts.Trader.UI.TraderHUD
{
    public class GameplayStatusUI : BaseBehaviour
    {

        private ITraderInfoService _infoService = null;
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

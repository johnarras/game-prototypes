using Assets.Scripts.Trader.ClientEvents;
using Assets.Scripts.Trader.Info.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Calendar.Services;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Constants;

namespace Assets.Scripts.Trader.UI.TraderHUD
{
    public class TraderStatusUI : BaseBehaviour
    {

        private ITraderInfoService _infoService = null;
        private ICalendarService _calendarService = null;

        public GText PositionText;
        public GText DateText;

        public override void Init()
        {
            base.Init();

            _dispatcher.AddListener<UpdateTraderStatusUI>(OnUpdateTraderStatusUI, GetToken());


            ShowData();

        }

        private void OnUpdateTraderStatusUI(UpdateTraderStatusUI update)
        {
            ShowData();
        }

        private void ShowData()
        {
            CoreData coreData = _gs.ch.Get<CoreData>();

            if (coreData != null)
            {
                _uiService.SetText(DateText, _calendarService.PrintDay(coreData.Vars[TraderVars.PlayCount]));
                _uiService.SetText(PositionText, _infoService.GetHUDStatus(coreData));
            }
        }
    }
}

using Assets.Scripts.Info.UI;
using OxDb.SharedCore.Entities.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.UI.InfoScreen
{
    public class TraderInfoScreen : BaseInfoScreen
    {
        public GButton CityButton;
        public GButton CaravanMemberButton;
        public GButton RoadButton;
        public GButton TradeGoodsButton;


        protected override string OverviewPath => "Text/TraderOverview";

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await base.OnStartOpen(data, token);

            _uiService.SetButton(CityButton, GetName(), () => ShowInfoList(EntityTypes.City));
            _uiService.SetButton(CaravanMemberButton, GetName(), () => ShowInfoList(EntityTypes.CaravanMember));
            _uiService.SetButton(RoadButton, GetName(), () => ShowInfoList(EntityTypes.Road));
            _uiService.SetButton(TradeGoodsButton, GetName(), () => ShowInfoList(EntityTypes.TradeGood));


        }
    }
}



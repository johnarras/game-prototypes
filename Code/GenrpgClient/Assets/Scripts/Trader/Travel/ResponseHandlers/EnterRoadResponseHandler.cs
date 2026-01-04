using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.Trader.ClientEvents;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Roads.WebApi;
using Genrpg.Shared.UI.Constants;
using System.Threading;

namespace Assets.Scripts.Trader.MessageHandlers.Travelling
{
    public class EnterRoadResponseHandler : BaseClientWebResponseHandler<EnterRoadResponse>
    {
        protected override void InnerProcess(EnterRoadResponse response, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                _dispatcher.Dispatch(new ShowFloatingText(response.ErrorMessage, EFloatingTextArt.Error));
                return;
            }

            CoreData coreData = _gs.ch.Get<CoreData>();

            coreData.Vars[TraderVars.RoadId] = response.RoadId;
            coreData.Vars[TraderVars.CityId] = response.TargetCityId;
            coreData.Vars[TraderVars.DistanceAlongRoad] = 0;

            _dispatcher.Dispatch(new CloseScreen(ScreenNames.TraderCityRoads));
            _dispatcher.Dispatch(new UpdateTraderStatusUI());
        }
    }
}

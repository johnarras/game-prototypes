using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.Trader.ClientEvents;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Cities.WebApi;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.UI.Constants;
using System.Threading;

namespace Assets.Scripts.Trader.MessageHandlers.Travelling
{
    public class EnterCityResponseHandler : BaseClientWebResponseHandler<EnterCityResponse>
    {
        protected override void InnerProcess(EnterCityResponse response, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                _dispatcher.Dispatch(new ShowFloatingText(response.ErrorMessage, EFloatingTextArt.Error));
                return;
            }

            CoreData coreData = _gs.ch.Get<CoreData>();

            coreData.Vars[TraderVars.CityId] = response.CityId;
            coreData.Vars[TraderVars.RoadId] = 0;
            coreData.Vars[TraderVars.DistanceAlongRoad] = 0;
            _dispatcher.Dispatch(new CloseScreen(ScreenNames.TraderCityRoads));
            _dispatcher.Dispatch(new UpdateTraderStatusUI());
        }
    }
}

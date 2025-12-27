using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Roads.WebApi;
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

            CoreUserData userData = _gs.ch.Get<CoreUserData>();

            userData.RoadId = response.RoadId;
            userData.CityId = response.TargetCityId;
            userData.Dist = 0;
        }
    }
}

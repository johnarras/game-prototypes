using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Roads.WebApi;
using System.Threading;

namespace Assets.Scripts.Trader.MessageHandlers.Travelling
{
    public class TurnAroundResponseHandler : BaseClientWebResponseHandler<TurnAroundResponse>
    {
        protected override void InnerProcess(TurnAroundResponse response, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                _dispatcher.Dispatch(new ShowFloatingText(response.ErrorMessage, EFloatingTextArt.Error));
                return;
            }

            CoreUserData userData = _gs.ch.Get<CoreUserData>();

            userData.CityId = response.TargetCityId;
            userData.Dist = response.DistanceTravelled;
        }
    }
}

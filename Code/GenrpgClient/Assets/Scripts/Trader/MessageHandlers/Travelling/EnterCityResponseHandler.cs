using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Cities.WebApi;
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

            CoreUserData userData = _gs.ch.Get<CoreUserData>();

            userData.CityId = response.CityId;
            userData.RoadId = 0;
            userData.Dist = 0;
        }
    }
}

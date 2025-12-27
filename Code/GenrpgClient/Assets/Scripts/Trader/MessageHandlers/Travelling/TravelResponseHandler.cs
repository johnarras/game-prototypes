using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Travel.WebApi;
using System.Threading;

namespace Assets.Scripts.Trader.MessageHandlers.Travelling
{
    public class TravelResponseHandler : BaseClientWebResponseHandler<TravelResponse>
    {
        protected override void InnerProcess(TravelResponse response, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                _dispatcher.Dispatch(new ShowFloatingText(response.ErrorMessage, EFloatingTextArt.Error));
                return;
            }

            CoreUserData userData = _gs.ch.Get<CoreUserData>();

            _dispatcher.Dispatch(response);
        }
    }
}

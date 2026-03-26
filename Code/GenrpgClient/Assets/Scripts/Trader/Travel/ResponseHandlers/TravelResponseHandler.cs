using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.FloatingText.ClientEvents;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Travel.WebApi;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.MessageHandlers.Travelling
{
    public class TravelResponseHandler : BaseClientWebResponseHandler<TravelResponse>
    {
        protected override async Awaitable InnerProcess(TravelResponse response, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                _dispatcher.Dispatch(new ShowFloatingText(response.ErrorMessage, EFloatingTextArt.Error));
                return;
            }

            CoreData coreData = _gs.ch.Get<CoreData>();

            _dispatcher.Dispatch(response);
            await Task.CompletedTask;
        }
    }
}

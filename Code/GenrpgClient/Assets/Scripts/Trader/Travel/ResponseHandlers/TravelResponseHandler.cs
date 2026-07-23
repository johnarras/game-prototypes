using OxDb.Client.ClientEvents.UI;
using OxDb.Client.FloatingText.ClientEvents;
using OxDb.Client.Login.Messages.Core;
using OxDb.Client.UI.Interfaces;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Trader.Travel.WebApi;
using OxDb.SharedGame.UI.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Trader.MessageHandlers.Travelling
{
    public class TravelResponseHandler : BaseClientWebResponseHandler<TravelResponse>
    {
        protected IUIService _uiService = null;
        protected override async ValueTask InnerProcess(TravelResponse response, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                _dispatcher.Dispatch(new ShowFloatingText(response.ErrorMessage, EFloatingTextArt.Error));
                _uiService.DecrementButtonBlock();
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.Caravan));
                return;
            }

            CoreData coreData = _gs.ch.Get<CoreData>();

            _dispatcher.Dispatch(response);
            await Task.CompletedTask;
        }
    }
}

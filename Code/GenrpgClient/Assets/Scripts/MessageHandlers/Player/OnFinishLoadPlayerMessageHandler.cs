
using OxDb.Client.ClientEvents.UI;
using OxDb.SharedGame.Players.Messages;
using OxDb.SharedGame.UI.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.MessageHandlers.Player
{
    public class OnFinishLoadPlayerMessageHandler : BaseClientMapMessageHandler<OnFinishLoadPlayer>
    {
        protected IScreenService _screenService = null;
        protected override async ValueTask InnerProcess(OnFinishLoadPlayer msg, CancellationToken token)
        {
            _dispatcher.Dispatch(msg);
            _dispatcher.Dispatch(new CloseAllScreens());
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.HUD));
            await Task.CompletedTask;
        }
    }
}



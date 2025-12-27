
using Assets.Scripts.ClientEvents.UI;
using Genrpg.Shared.Players.Messages;
using Genrpg.Shared.UI.Constants;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Player
{
    public class OnFinishLoadPlayerMessageHandler : BaseClientMapMessageHandler<OnFinishLoadPlayer>
    {
        protected IScreenService _screenService = null;
        protected override void InnerProcess(OnFinishLoadPlayer msg, CancellationToken token)
        {
            _dispatcher.Dispatch(msg);
            _dispatcher.Dispatch(new CloseAllScreens());
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.HUD));
        }
    }
}



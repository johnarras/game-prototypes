using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.UI.Screens;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Website.Messages.Error;
using System.Collections.Generic;
using System.Threading;
using Assets.Scripts.UI.Entities;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class ErrorResponseHandler : BaseClientWebResponseHandler<ErrorResponse>
    {
        private IScreenService _screenService;
        protected override void InnerProcess(ErrorResponse result, CancellationToken token)
        {

            List<ActiveScreen> screens = _screenService.GetAllScreens();

            bool foundErrorScreen = false;

            foreach (ActiveScreen screen in screens)
            {
                if (screen.Screen is ErrorMessageScreen errorScreen)
                {
                    errorScreen.ShowError(result.Error);
                    foundErrorScreen = true;
                }
            }

            if (foundErrorScreen)
            {
                return;
            }

            _screenService.CloseAll();
            _screenService.Open(ScreenNames.Login);

            _dispatcher.Dispatch(new ShowFloatingText(result.Error, EFloatingTextArt.Error));

            _logService.Error(result.Error);
        }
    }
}

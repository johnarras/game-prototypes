using OxDb.Client.ClientEvents.UI;
using OxDb.Client.FloatingText.ClientEvents;
using OxDb.Client.Login.Messages.Core;
using OxDb.Client.UI.Entities;
using OxDb.Client.UI.Screens;
using OxDb.SharedCore.Website.Responses.Errors;
using OxDb.SharedGame.UI.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Website.MessageHandlers
{
    public class ErrorResponseHandler : BaseClientWebResponseHandler<ErrorResponse>
    {
        private IScreenService _screenService = null;
        protected override async ValueTask InnerProcess(ErrorResponse response, CancellationToken token)
        {
            List<ActiveScreen> screens = _screenService.GetAllScreens();

            bool foundErrorScreen = false;

            foreach (ActiveScreen screen in screens)
            {
                if (screen.Screen is ErrorMessageScreen errorScreen)
                {
                    errorScreen.ShowError(response.Error);
                    foundErrorScreen = true;
                }
            }

            if (foundErrorScreen)
            {
                return;
            }

            _dispatcher.Dispatch(new CloseAllScreens() { CloseKeepOpenScreens = true });
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.GetMainAuthScreen(), response));

            _dispatcher.Dispatch(new ShowFloatingText(response.Error, EFloatingTextArt.Error));

            _logService.Error(response.Error);

            await Task.CompletedTask;
        }
    }
}



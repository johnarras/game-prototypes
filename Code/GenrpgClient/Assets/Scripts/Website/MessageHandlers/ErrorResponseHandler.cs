using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.FloatingText.ClientEvents;
using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.UI.Entities;
using Assets.Scripts.UI.Screens;
using OxDb.SharedCore.Website.Responses.Errors;
using OxDb.SharedGame.UI.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class ErrorResponseHandler : BaseClientWebResponseHandler<ErrorResponse>
    {
        private IScreenService _screenService = null;
        protected override async Awaitable InnerProcess(ErrorResponse response, CancellationToken token)
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
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.Login, response));

            _dispatcher.Dispatch(new ShowFloatingText(response.Error, EFloatingTextArt.Error));

            _logService.Error(response.Error);

            await Task.CompletedTask;
        }
    }
}



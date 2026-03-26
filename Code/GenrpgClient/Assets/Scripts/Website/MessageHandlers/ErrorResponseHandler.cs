using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.UI.Entities;
using Assets.Scripts.UI.Screens;
using Assets.Scripts.FloatingText.ClientEvents;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Website.Messages.Error;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class ErrorResponseHandler : BaseClientWebResponseHandler<ErrorResponse>
    {
        private IScreenService _screenService = null;
        protected override async Awaitable InnerProcess(ErrorResponse result, CancellationToken token)
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

            _dispatcher.Dispatch(new CloseAllScreens());
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.Login));

            _dispatcher.Dispatch(new ShowFloatingText(result.Error, EFloatingTextArt.Error));

            _logService.Error(result.Error);

            await Task.CompletedTask;
        }
    }
}



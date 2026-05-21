
using Assets.Scripts.ClientEvents.UI;
using OxDb.SharedGame.UI.Constants;
using System.Threading;
using UnityEngine;

public class AfterGenerateMap : BaseZoneGenerator
{

    protected IScreenService _screenService = null;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        if (string.IsNullOrEmpty(_zoneGenService.LoadedMapId))
        {
            _dispatcher.Dispatch(new CloseAllScreens());
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.CharacterSelect));
        }

    }


}




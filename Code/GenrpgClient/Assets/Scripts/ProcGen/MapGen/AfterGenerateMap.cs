
using System.Threading;
using UnityEngine;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.UI.Constants;

public class AfterGenerateMap : BaseZoneGenerator
{

    protected IScreenService _screenService;
    public override async Awaitable Generate (CancellationToken token)
    {
        await base.Generate(token);

        if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId))
        {
            _screenService.CloseAll();
            _screenService.Open(ScreenNames.CharacterSelect);
        }
        
	}
	

}
	

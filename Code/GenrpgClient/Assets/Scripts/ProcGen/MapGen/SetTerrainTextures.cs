

using System.Threading;
using UnityEngine; // Needed

public class SetTerrainTextures : BaseZoneGenerator
{

    private ITerrainTextureManager _terrainTextureManager;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        await _terrainTextureManager.DownloadAllTerrainTextures(token);

        for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
        {
            for (int gz = 0; gz < _mapProvider.GetMap().BlockCount; gz++)
            {
                _awaitableService.ForgetAwaitable(_terrainManager.InitTerrainContainer(_terrainManager.GetTerrainPatch(gx, gz, true), token));
            }
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
        await Awaitable.WaitForSecondsAsync(2.0f, cancellationToken: token);
    }
}


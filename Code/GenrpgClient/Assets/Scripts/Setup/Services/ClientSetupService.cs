
using Genrpg.Shared.Client.Tokens;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Setup.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ClientSetupService : SetupService
{
    public override async Task SetupGame(IGameState gs, List<object> existingObjects, CancellationToken token)
    {
        await base.SetupGame(gs, existingObjects, token);

        foreach (IGameTokenService service in gs.loc.GetVals<IGameTokenService>())
        {
            service.SetGameToken(token);
        }


    }
}



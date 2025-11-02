
using Genrpg.Shared.Client.Tokens;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Setup.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ClientSetupService : SetupService
{
    public async Task FinalInitialize(IServiceLocator loc, CancellationToken token)
    {
        loc.ResolveSelf();

        List<Task> setupTasks = new List<Task>();

        foreach (IInitializable service in loc.GetVals<IInitializable>())
        {
            setupTasks.Add(service.Initialize(token));
        }

        await Task.WhenAll(setupTasks);

        foreach (IGameTokenService service in loc.GetVals<IGameTokenService>())
        {
            service.SetGameToken(token);
        }
    }
}

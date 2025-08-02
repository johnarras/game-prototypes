
using Genrpg.Shared.Client.Tokens;
using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Setup.Services;
using System.Collections.Generic;

public class ClientSetupService : SetupService
{
    public async Task FinalInitialize(IServiceLocator loc, CancellationToken token)
    {
        loc.ResolveSelf();

        List<IInjectable> vals = loc.GetVals();

        foreach (IInjectable service in vals)
        {
            if (service is IInitializable initService)
            {
                await initService.Initialize(token);
            }
        }
        List<Task> setupTasks = new List<Task>();

        foreach (IInjectable service in loc.GetVals())
        {
            if (service is IInitializable initService)
            {
                setupTasks.Add(initService.Initialize(token));
            }
        }

        await Task.WhenAll(setupTasks);

        foreach (IInjectable service in loc.GetVals())
        {
            if (service is IGameTokenService gameTokenService)
            {
                gameTokenService.SetGameToken(token);
            }
        }
    }
}

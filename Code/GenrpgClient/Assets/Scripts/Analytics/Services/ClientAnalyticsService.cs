using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ClientAnalyticsService : IAnalyticsService
{

    private ILogService _logService;
    private ITextSerializer _serializer;
    private IClientConfigContainer _configContainer = null;
    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }
    public void Send(string eventId, string eventType, string eventSubtype, Dictionary<string, string> extraData = null)
    {
        if (_configContainer.Config.SelfContainedClient)
        {
            return;
        }
    }

}



using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ClientAnalyticsService : IAnalyticsService
{

    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    private ClientConfig _config = null;
    private ITextSerializer _serializer = null;
    private ILogService _logService = null;
    public ClientAnalyticsService (ClientConfig config, ILogService logService, ITextSerializer serializer)
    {
        _config = config;
        _serializer = serializer;
        _logService = logService;

        if (!_config.SelfContainedClient)
        {
            // Do setup.
        }
    }

    public void Send(string eventId, string eventType, string eventSubtype, Dictionary<string,string> extraData = null)
    {
        if (_config.SelfContainedClient)
        {
            return;
        }
    }

}

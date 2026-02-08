using Genrpg.ServerShared.Config;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Serialization.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Analytics.Services
{
    public class ServerAnalyticsService : IAnalyticsService
    {
        public async Task Initialize(CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        public void Send(string eventId, string eventType, string eventSubtype, Dictionary<string, string> extraData = null)
        {
        }

    }
}



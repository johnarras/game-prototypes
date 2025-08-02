using Genrpg.ServerShared.Config;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Analytics.Services
{
    public class ServerAnalyticsService : IAnalyticsService
    {

        private ITextSerializer _serializer;
        public async Task Initialize(CancellationToken toke)
        {
            await Task.CompletedTask;
        }


        private IServerConfig _serverConfig = null;
        public ServerAnalyticsService(IServerConfig serverConfig, ITextSerializer serializer)
        {
            _serverConfig = serverConfig;
            _serializer = serializer;
        }


        public void Send(string eventId, string eventType, string eventSubtype, Dictionary<string,string> extraData = null)
        {
        }

    }
}

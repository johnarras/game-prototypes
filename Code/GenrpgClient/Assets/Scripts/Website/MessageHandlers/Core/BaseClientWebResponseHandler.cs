
using OxDb.Client.Awaitables;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Website.Responses.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Login.Messages.Core
{
    public abstract class BaseClientWebResponseHandler<T> : IClientWebResponseHandler where T : class, IWebResponse
    {
        protected ILogService _logService = null;
        protected IRepositoryService _repoService = null;
        protected IDispatcher _dispatcher = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IAwaitableService _awaitableService = null;
        public Type HelperKey => typeof(T);

        virtual public int Priority() { return 0; }

        protected abstract ValueTask InnerProcess(T response, CancellationToken token);

        public async ValueTask Process(IWebResponse response, CancellationToken token)
        {
            await InnerProcess(response as T, token);
        }
    }
}



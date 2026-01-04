
using Assets.Scripts.Awaitables;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Threading;

namespace Assets.Scripts.Login.Messages.Core
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

        protected abstract void InnerProcess(T response, CancellationToken token);

        public void Process(IWebResponse response, CancellationToken token)
        {
            InnerProcess(response as T, token);
        }
    }
}



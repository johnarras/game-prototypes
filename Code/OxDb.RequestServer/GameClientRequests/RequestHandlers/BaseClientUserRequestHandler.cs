using OxDb.RequestServer.Core;
using OxDb.RequestServer.PlayerData.Services;
using OxDb.ServerCore.Config;
using OxDb.ServerGame.PlayerData.Services;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Website.Interfaces;
using OxDb.SharedCore.Website.Requests.Interfaces;
using OxDb.SharedCore.Website.Responses.Errors;

namespace OxDb.RequestServer.GameClientRequests.RequestHandlers
{
    public abstract class BaseClientUserRequestHandler<TRequest> : IGameClientRequestHandler where TRequest : IClientUserRequest
    {

        protected IPlayerDataService _playerDataService = null;
        protected ILoadPlayerDataService _loginPlayerDataService = null;
        protected ILogService _logService = null;
        protected IRepositoryService _repoService = null;
        protected IServerConfig _config = null;
        protected ITextSerializer _serializer = null;

        protected abstract Task InnerHandleMessage(WebContext context, TRequest request, CancellationToken token);

        public Type HelperKey => typeof(TRequest);

        public virtual async Task Reset()
        {
            await Task.CompletedTask;
        }

        public async Task Execute(WebContext context, IWebRequest request, CancellationToken token)
        {
            await InnerHandleMessage(context, (TRequest)request, token);
        }

        protected void ShowError(WebContext context, string msg)
        {
            context.AddResponse(new ErrorResponse() { Error = msg });
        }
    }

}



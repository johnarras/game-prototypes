using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayerData.Services;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.ServerShared.Accounts.Services;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Crypto.Services;
using Genrpg.ServerShared.DataStores;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages.Error;

namespace Genrpg.RequestServer.AuthRequests.GameAuthRequestHandlers
{
    public abstract class BaseGameAuthRequestHandler<TRequest> : IGameAuthRequestHandler where TRequest : class, IGameAuthRequest
    {
        protected ITextSerializer _serializer = null!;
        protected IPlayerDataService _playerDataService = null!;
        protected ILoginPlayerDataService _loginPlayerDataService = null!;
        protected ILogService _logService = null!;
        protected IServerRepositoryService _serverRepoService = null!;
        protected IServerConfig _config = null!;
        protected IWebServerService _loginServerService = null!;
        protected IGameDataService _gameDataService = null!;
        protected ICloudCommsService _cloudCommsService = null!;
        protected IWebServerService _webServerService = null!;
        protected IAccountService _accountService = null!;
        protected ICryptoService _cryptoService = null!;



        protected abstract Task HandleRequestInternal(WebContext context, TRequest request, CancellationToken token);

        public Type Key => typeof(TRequest);

        public virtual async Task Reset()
        {
            await Task.CompletedTask;
        }

        public async Task Execute(WebContext context, IGameAuthRequest request, CancellationToken token)
        {
            await HandleRequestInternal(context, (TRequest)request, token);
        }
        protected void ShowError(WebContext context, string msg)
        {
            context.Responses.AddResponse(new ErrorResponse() { Error = msg });
        }

    }
}

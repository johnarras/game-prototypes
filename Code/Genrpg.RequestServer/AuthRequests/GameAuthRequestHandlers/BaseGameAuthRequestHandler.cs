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
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
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
        protected IFullRepositoryService _serverRepoService = null!;
        protected IServerConfig _config = null!;
        protected IWebServerService _loginServerService = null!;
        protected IServerGameDataService _gameDataService = null!;
        protected ICloudCommsService _cloudCommsService = null!;
        protected IWebServerService _webServerService = null!;
        protected IAccountService _accountService = null!;
        protected ICryptoService _cryptoService = null!;



        protected abstract Task HandleRequestInternal(WebContext context, TRequest request, CancellationToken token);

        public Type HelperKey => typeof(TRequest);

        public virtual async Task Reset()
        {
            await Task.CompletedTask;
        }

        public async Task Execute(WebContext context, IGameAuthRequest request, CancellationToken token)
        {
            try
            {
                await HandleRequestInternal(context, (TRequest)request, token);
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "GameAuth.Execute");
            }
        }
        protected void ShowError(WebContext context, string msg)
        {
            context.AddResponse(new ErrorResponse() { Error = msg });
        }

    }
}



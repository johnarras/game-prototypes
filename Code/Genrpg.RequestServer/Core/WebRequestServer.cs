
using Genrpg.RequestServer.Core.Services;
using Genrpg.RequestServer.RequestHandlers;
using Genrpg.RequestServer.Services.AccountAuth;
using Genrpg.RequestServer.Services.GameAuth;
using Genrpg.RequestServer.Services.GameClient;
using Genrpg.RequestServer.Services.NoUsers;
using Genrpg.RequestServer.Setup;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Crypto.Services;
using Genrpg.ServerShared.MainServer;
using Genrpg.Shared.Charms.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages;

namespace Genrpg.RequestServer.Core
{
    /// <summary>
    /// This is a minimal amount of webdev used to get us into code that can be used elsewhere easier.
    /// </summary>
    public class WebRequestServer : BaseServer<WebContext, WebsiteSetupService, IWebsiteQueueMessageHandler>
    {
        protected IGameClientWebService _gameClientWebService { get; private set; }
        protected IAccountAuthWebService _accountAuthWebService { get; private set; }
        protected IGameAuthWebService _gameAuthWebService { get; private set; }
        protected ICryptoService _cryptoService { get; private set; }
        protected ICharmService _charmService { get; private set; }
        protected INoUserWebService _noUserWebService { get; private set; }
        protected IRepositoryService _repositoryService { get; private set; }
        protected ITextSerializer _textSerializer { get; private set; }
        protected IBinarySerializer _binarySerializer { get; private set; }
        private CancellationTokenSource _serverSource = new CancellationTokenSource();
        protected CancellationToken _token => _serverSource.Token;
        protected IPartitionedDataSaveService _saveService { get; private set; }

        protected IServerConfig _serverConfig { get; private set; }

        public WebRequestServer()
        {
            _serverSource = new CancellationTokenSource();

            Init(_serverSource.Token).Wait();
        }

        public string GetIndexString()
        {
            return "[ Index: " + _serverConfig.DefaultEnv + " ]";
        }

        protected WebContext SetupContext()
        {
            return new WebContext(_config, _context.loc, _repositoryService, _binarySerializer, _saveService);
        }

        protected string _serverInstanceId = CloudServerNames.Login + HashUtils.NewGuid().ToString().ToLowerInvariant();
        protected override string GetServerId(object data)
        {
            return _serverInstanceId;
        }

        public async Task<string> HandleUserClient(WebServerRequestSet postData, string sessionUserId)
        {
            WebContext context = SetupContext();
            await _gameClientWebService.HandleUserClientRequest(context, postData, _token);
            return PackageResponses(context);
        }
        public async Task<string> HandleRefreshToken(WebServerRequestSet postData)
        {
            WebContext context = SetupContext();
            await _gameAuthWebService.HandleRefreshTokenRequest(context, postData, _token);
            return PackageResponses(context);
        }

        public async Task<string> HandleNoUser(WebServerRequestSet postData)
        {
            WebContext context = SetupContext();
            await _noUserWebService.HandleNoUserRequest(context, postData, _token);
            return PackageResponses(context);
        }

        public async Task<string> HandleAccountAuth(WebServerRequestSet postData)
        {
            WebContext context = SetupContext();
            await _accountAuthWebService.HandleAccountAuthRequest(context, postData, _token);
            return PackageResponses(context);
        }


        public async Task<string> HandleGameAuth(WebServerRequestSet postData)
        {
            WebContext context = SetupContext();
            await _gameAuthWebService.HandleGameAuthRequest(context, postData, _token);
            return PackageResponses(context);
        }

        private string PackageResponses(WebContext context)
        {
            string txt = _textSerializer.SerializeToString(new WebServerResponseSet() { Responses = context.GetResponseList() });

            context.Dispose();

            return txt;
        }

    }
}



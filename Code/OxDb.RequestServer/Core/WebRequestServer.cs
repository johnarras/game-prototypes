using OxDb.PlatformServer.Accounts.Services;
using OxDb.PlatformServer.Entities;
using OxDb.RequestServer.Core.Entities;
using OxDb.RequestServer.Core.Services;
using OxDb.RequestServer.GameAuthRequests.Services;
using OxDb.RequestServer.GameClientRequests.Services;
using OxDb.RequestServer.Queues;
using OxDb.RequestServer.Setup;
using OxDb.ServerCore.Constants;
using OxDb.ServerCore.Crypto.Services;
using OxDb.ServerCore.MainServer;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Website.Requests.Core;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedGame.Charms.Services;

namespace OxDb.RequestServer.Core
{
    /// <summary>
    /// This is a minimal amount of webdev used to get us into code that can be used elsewhere easier.
    /// </summary>
    public class WebRequestServer : BaseServer<WebContext, WebsiteSetupService, IWebsiteQueueMessageHandler>
    {
        protected IGameClientRequestService _gameClientWebService { get; private set; }
        protected IAccountAuthWebService _accountAuthWebService { get; private set; }
        protected IGameAuthWebService _gameAuthWebService { get; private set; }
        protected ICryptoService _cryptoService { get; private set; }
        protected ICharmService _charmService { get; private set; }
        protected IRepositoryService _repositoryService { get; private set; }
        protected ITextSerializer _textSerializer { get; private set; }
        protected IBinarySerializer _binarySerializer { get; private set; }
        private CancellationTokenSource _serverSource = new CancellationTokenSource();
        protected CancellationToken _token => _serverSource.Token;
        protected IPartitionedDataSaveService _saveService { get; private set; }

        public WebRequestServer()
        {
        }

        public ILogService GetLogService()
        {
            return _logService;
        }

        public void Init(List<IInjectable> initialServices)
        {
            _serverSource = new CancellationTokenSource();

            ServerInitArgs args = new ServerInitArgs(initialServices, _serverSource.Token, null);
            try
            {
                Init(args).Wait();
            }
            catch (Exception e)
            {
                _logService.Exception(e, "TopLevelInit");
            }
        }

        public string GetIndexString()
        {
            return "[ Index: " + _config.Env + " ]";
        }

        protected WebContext SetupContext()
        {
            return new WebContext(_config, _logService, _gameState.loc, _repositoryService, _binarySerializer, _saveService);
        }

        protected override bool UseInstanceId => true;
        protected override string GetBaseServerName() { return ServerNames.Website; }

        public async Task<string> HandleUserClient(WebServerRequestSet postData, UserWebRequestClaimData claimData)
        {
            WebContext context = SetupContext();
            context.SetCurrentData(claimData.ExistingData, postData.ClientVersion, postData.ClientPlatform);
            await _gameClientWebService.HandleUserClientRequest(context, postData, claimData.UserId, claimData.GameSessionId, _token);
            return PackageResponses(context);
        }
        public async Task<string> HandleRefreshToken(WebServerRequestSet postData)
        {
            WebContext context = SetupContext();
            await _gameAuthWebService.HandleRefreshTokenRequest(context, postData, _token);
            return PackageResponses(context);
        }

        public async Task<string> HandleAccountAuth(WebServerRequestSet postData)
        {
            AuthWebContext context = new AuthWebContext();
            await _accountAuthWebService.HandleAccountAuthRequest(context, postData, _token);
            return PackageResponses(context);
        }


        public async Task<string> HandleGameAuth(WebServerRequestSet postData)
        {
            WebContext context = SetupContext();
            await _gameAuthWebService.HandleGameAuthRequest(context, postData, _token);
            return PackageResponses(context);
        }

        private string PackageResponses(IWebContext context)
        {
            string txt = _textSerializer.SerializeToString(new WebServerResponseSet() { Responses = context.GetResponseList() });

            context.Dispose();

            return txt;
        }

    }
}



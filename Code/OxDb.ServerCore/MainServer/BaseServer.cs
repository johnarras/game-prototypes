using Microsoft.Extensions.Hosting;
using OxDb.ServerCore.AzureImpl.Secrets.Services;
using OxDb.ServerCore.CloudComms.PubSub.Topics.Admin.Messages;
using OxDb.ServerCore.CloudComms.Queues.Entities;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.ServerCore.Config;
using OxDb.ServerCore.Core;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Setup.Services;

namespace OxDb.ServerCore.MainServer
{
    public interface IBaseServer
    {
        CancellationToken GetToken();
        ServerGameState GetServerGameState();
    }

    public class ServerInitArgs
    {
        public CancellationToken Token;
        public object Data = null;
        public object Parent = null;
        public string EnvOverride = null;
        public List<IInjectable> InitialServices = new List<IInjectable>();

        public ServerInitArgs(List<IInjectable> initialServices, CancellationToken token, object data = null, object parent = null, string envOverride = null)
        {
            InitialServices = initialServices;
            Token = token;
            Data = data;
            Parent = parent;
            EnvOverride = envOverride;
        }

    }

    public abstract class BaseServer<TGameState, TSetupService, IQMessageHandler> : IBaseServer
        where TGameState : ServerGameState
        where TSetupService : SetupService
        where IQMessageHandler : IQueueMessageHandler
    {
        protected TGameState _gameState = null!;
        protected CancellationTokenSource _currServerToken = new CancellationTokenSource();
        protected string _serverId = null;
        protected ICloudCommsService _cloudCommsService = null!;
        protected IServerConfig _config = null!;
        protected ILogService _logService = null!;
        protected IHostApplicationBuilder _builder = null!;


        protected string _instanceId = Random.Shared.Next().ToString();
        protected abstract bool UseInstanceId { get; }
        protected abstract string GetBaseServerName();

        protected string GetFullServerName(object data)
        {
            return GetBaseServerName() + (UseInstanceId ? _instanceId : "");
        }


        public virtual CancellationToken GetToken()
        {
            return _currServerToken?.Token ?? CancellationToken.None;
        }

        public virtual ServerGameState GetServerGameState()
        {
            return _gameState;
        }

        protected virtual async Task PreInit(ServerInitArgs args)
        {
            await Task.CompletedTask;
        }


        protected virtual async Task PostInit(ServerInitArgs args)
        {
            await Task.CompletedTask;
        }

        private SetupDictionaryContainer<Type, IQMessageHandler> _queueHandlers = new();

        public async Task Init(ServerInitArgs args)
        {
            _currServerToken = CancellationTokenSource.CreateLinkedTokenSource(args.Token);
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                _currServerToken.Cancel();
            };

            await PreInit(args);

            _serverId = GetFullServerName(args.Data);

            _gameState = await SetupFromConfig<TGameState>(this, args.InitialServices, _serverId, _currServerToken.Token);

            _cloudCommsService.SetQueueMessageHandlers(_queueHandlers.GetDict());

            _cloudCommsService.SendPubSubMessage(new ServerStartedAdminMessage() { ServerName = _serverId });

            _gameState.loc.Resolve(args.Parent);
            _gameState.loc.Resolve(this);
            await PostInit(args);
            _config.ClearSecretsAfterInit();

        }

        public async Task<GS> SetupFromConfig<GS>(object currentObject, List<IInjectable> initialServices, string serverId, CancellationToken token)
          where GS : ServerGameState
        {
            if (string.IsNullOrEmpty(serverId))
            {
                throw new Exception("Missing ServerName in setup code!");
            }

            IServerConfig config = await SetupServerConfig(token, serverId);

            ILogService logService = null;

            foreach (IInjectable inj in initialServices)
            {
                if (inj is ILogService ls)
                {
                    logService = ls;
                    break;
                }
            }

            GS gs = (GS)Activator.CreateInstance(typeof(GS), new object[] { config, logService })!;
            TSetupService setupService = (TSetupService)Activator.CreateInstance(typeof(TSetupService))!;
            await setupService.SetupGame(gs, this, new List<object> { currentObject }, token);

            return gs;
        }

        public async Task<IServerConfig> SetupServerConfig(CancellationToken token, string serverId)
        {

            ServerConfig serverConfig = new ServerConfig();
            await serverConfig.Init<AzureSecretsClient>(serverId);

            return serverConfig;

        }
    }
}



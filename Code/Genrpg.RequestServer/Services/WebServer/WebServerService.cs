using Genrpg.RequestServer.AuthRequests.AccountAuthRequestHandlers;
using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Maps;
using Genrpg.RequestServer.NoUserRequests.RequestHandlers;
using Genrpg.ServerShared.Maps;
using Genrpg.Shared.HelperClasses;

namespace Genrpg.RequestServer.Services.WebServer
{
    public class WebServerService : IWebServerService
    {

        private IMapDataService _mapDataService = null!;

        private MapStubList _mapStubs { get; set; } = new MapStubList();
        private SetupDictionaryContainer<Type, IGameClientRequestHandler> _clientCommandHandlers = new SetupDictionaryContainer<Type, IGameClientRequestHandler>();
        private SetupDictionaryContainer<Type, INoUserRequestHandler> _noUserCommandHandlers = new SetupDictionaryContainer<Type, INoUserRequestHandler>();
        private SetupDictionaryContainer<Type, IAccountAuthRequestHandler> _accountAuthCommandHandlers = new SetupDictionaryContainer<Type, IAccountAuthRequestHandler>();


        public async Task Initialize(CancellationToken token)
        {
            _mapStubs.Stubs = await _mapDataService.GetMapStubs();
            await Task.CompletedTask;
        }

        public MapStubList GetMapStubs()
        {
            return _mapStubs;
        }

        public IAccountAuthRequestHandler GetAccountAuthRquestHandler(Type type)
        {
            if (_accountAuthCommandHandlers.TryGetValue(type, out IAccountAuthRequestHandler handler))
            {
                return handler;
            }
            return null;
        }

        public IGameClientRequestHandler GetGameClientRequestHandler(Type type)
        {
            if (_clientCommandHandlers.TryGetValue(type, out IGameClientRequestHandler commandHandler))
            {
                return commandHandler;
            }

            return null;
        }

        public INoUserRequestHandler GetNoUserCommandHandler(Type type)
        {
            if (_noUserCommandHandlers.TryGetValue(type, out INoUserRequestHandler commandHandler))
            {
                return commandHandler;
            }
            return null;
        }

        public async Task ResetRequestHandlers()
        {
            foreach (IGameClientRequestHandler handler in _clientCommandHandlers.GetDict().Values)
            {
                await handler.Reset();
            }

            foreach (INoUserRequestHandler handler in _noUserCommandHandlers.GetDict().Values)
            {
                await handler.Reset();
            }

        }
    }
}



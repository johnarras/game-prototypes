using Genrpg.RequestServer.AuthRequests.AccountAuthRequestHandlers;
using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Maps;
using Genrpg.RequestServer.NoUserRequests.RequestHandlers;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Services.WebServer
{
    public interface IWebServerService : IInitializable
    {
        IGameClientRequestHandler GetGameClientRequestHandler(Type type);
        INoUserRequestHandler GetNoUserCommandHandler(Type type);
        IAccountAuthRequestHandler GetAccountAuthRquestHandler(Type type);
        Task ResetRequestHandlers();
        MapStubList GetMapStubs();
    }
}



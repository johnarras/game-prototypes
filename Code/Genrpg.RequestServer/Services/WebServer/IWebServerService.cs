using Genrpg.RequestServer.AuthRequests.AccountAuthRequestHandlers;
using Genrpg.RequestServer.AuthRequests.GameAuthRequestHandlers;
using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Maps;
using Genrpg.RequestServer.NoUserRequests.RequestHandlers;
using Genrpg.Shared.Interfaces;
using System;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Services.WebServer
{
    public interface IWebServerService : IInitializable
    {
        IGameClientRequestHandler GetGameClientRequestHandler(Type type);
        INoUserRequestHandler GetNoUserCommandHandler(Type type);
        IAccountAuthRequestHandler GetAccountAuthRquestHandler(Type type);
        IGameAuthRequestHandler GetGameAuthRequestHandler(Type type);
        Task ResetRequestHandlers();
        MapStubList GetMapStubs();
    }
}

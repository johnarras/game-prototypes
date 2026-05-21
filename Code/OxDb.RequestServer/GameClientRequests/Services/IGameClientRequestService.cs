using OxDb.RequestServer.Core;
using OxDb.RequestServer.Maps;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Website.Requests.Core;

namespace OxDb.RequestServer.ClientUserRequests.Services
{
    public interface IGameClientRequestService : IInitializable
    {
        Task HandleUserClientRequest(WebContext context, WebServerRequestSet requestSet, string tokenUserId, CancellationToken token);
        Task ResetRequestHandlers();
        MapStubList GetMapStubs();
    }
}



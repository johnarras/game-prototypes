using OxDb.RequestServer.Core;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Website.Requests.Core;

namespace OxDb.RequestServer.GameAuthRequests.Services
{
    public interface IGameAuthWebService : IInitializable
    {
        Task HandleGameAuthRequest(WebContext context, WebServerRequestSet requestSet, CancellationToken token);
        Task HandleRefreshTokenRequest(WebContext context, WebServerRequestSet requestSet, CancellationToken token);
    }
}



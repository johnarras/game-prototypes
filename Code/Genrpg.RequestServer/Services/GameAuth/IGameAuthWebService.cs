using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Messages;

namespace Genrpg.RequestServer.Services.GameAuth
{
    public interface IGameAuthWebService : IInjectable
    {
        Task HandleGameAuthRequest(WebContext context, WebServerRequestSet requestSet, CancellationToken token);
        Task HandleRefreshTokenRequest(WebContext context, WebServerRequestSet requestSet, CancellationToken token);
    }
}



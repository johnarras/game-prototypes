using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Messages;

namespace Genrpg.RequestServer.Services.GameClient
{
    public interface IGameClientWebService : IInjectable
    {
        Task HandleUserClientRequest(WebContext context, WebServerRequestSet requestSet, CancellationToken token);
    }
}



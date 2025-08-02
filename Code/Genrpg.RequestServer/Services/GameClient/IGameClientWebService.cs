using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Services.GameClient
{
    public interface IGameClientWebService : IInjectable
    {
        Task HandleUserClientRequest(WebContext context, string postData, CancellationToken token);
    }
}

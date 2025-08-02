using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Services.GameAuth
{
    public interface IGameAuthWebService : IInjectable
    {
        Task HandleGameAuthRequest(WebContext context, string postData, CancellationToken token);
    }
}

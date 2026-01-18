using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Messages;

namespace Genrpg.RequestServer.Services.AccountAuth
{
    public interface IAccountAuthWebService : IInjectable
    {
        Task HandleAccountAuthRequest(WebContext context, WebServerRequestSet requestSet, CancellationToken token);
    }
}



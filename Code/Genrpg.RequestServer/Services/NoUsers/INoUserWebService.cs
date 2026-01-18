using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Messages;

namespace Genrpg.RequestServer.Services.NoUsers
{
    public interface INoUserWebService : IInjectable
    {
        Task HandleNoUserRequest(WebContext context, WebServerRequestSet requestSet, CancellationToken token);
    }
}



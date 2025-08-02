using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Services.AccountAuth
{
    public interface IAccountAuthWebService : IInjectable
    {
        Task HandleAccountAuthRequest(WebContext context, string postData, CancellationToken token);
    }
}

using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Website.Requests.Core;
using OxDb.SharedCore.Website.Responses.Core;

namespace OxDb.PlatformServer.Accounts.Services
{
    public interface IAccountAuthWebService : IInjectable
    {
        Task HandleAccountAuthRequest(IWebContext context, WebServerRequestSet requestSet, CancellationToken token);
    }
}



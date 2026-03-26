using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.RequestServer.AuthRequests.AccountAuthRequestHandlers
{
    public interface IAccountAuthRequestHandler : ISetupDictionaryItem<Type>
    {
        Task Reset();
        Task Execute(WebContext context, IAccountAuthRequest request, CancellationToken token);
    }
}



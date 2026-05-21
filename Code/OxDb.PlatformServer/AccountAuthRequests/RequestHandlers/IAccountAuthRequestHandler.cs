using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Website.Interfaces;
using OxDb.SharedCore.Website.Responses.Core;

namespace OxDb.PlatformServer.AccountAuthRequests.RequestHandlers
{
    public interface IAccountAuthRequestHandler : ISetupDictionaryItem<Type>
    {
        Task Reset();
        Task Execute(IWebContext context, IAccountAuthRequest request, CancellationToken token);
    }
}



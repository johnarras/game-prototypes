using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;

namespace OxDb.PlatformServer.Accounts.RequestHandlers
{
    public interface IAccountAuthRequestHandler : ISetupDictionaryItem<Type>
    {
        Task Execute(IWebContext context, IAccountAuthRequest request, CancellationToken token);
    }
}



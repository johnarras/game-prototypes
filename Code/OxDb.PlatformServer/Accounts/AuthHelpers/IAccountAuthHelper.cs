using OxDb.PlatformServer.Accounts.Entities;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedPlatform.Accounts.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;

namespace OxDb.PlatformServer.Accounts.AuthHelpers
{
    public interface IAccountAuthHelper : ISetupDictionaryItem<EAuthTypes>, IInitOnResolve
    {
        Task<AccountAuthResult> CheckAuthAsync(IWebContext context, IAccountAuthRequest request, CancellationToken token);
    }
}

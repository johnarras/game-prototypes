using OxDb.PlatformServer.Accounts.Entities;
using OxDb.PlatformServer.Accounts.Services;
using OxDb.ServerCore.Crypto.Services;
using OxDb.ServerCore.DataStores.Services;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedPlatform.Accounts.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;

namespace OxDb.PlatformServer.Accounts.AuthHelpers
{
    public abstract class BaseAccountAuthHelper : IAccountAuthHelper
    {
        protected ICryptoService _cryptoService = null;
        protected IFullRepositoryService _repoService = null;
        protected IAccountService _accountService = null;

        public abstract EAuthTypes HelperKey { get; }

        protected AccountAuthResult CreateAuthResult()
        {
            return new AccountAuthResult(HelperKey);
        }

        public abstract Task<AccountAuthResult> CheckAuthAsync(IWebContext context, IAccountAuthRequest request);
    }
}

using OxDb.Client.Auth.Services;
using OxDb.Client.Login.Messages.Core;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Auth.ResponseHandlers
{
    public class AccountAuthResponseHandler : BaseClientWebResponseHandler<AccountAuthResponse>
    {
        private IClientAuthService _authService = null;
        protected override async ValueTask InnerProcess(AccountAuthResponse response, CancellationToken token)
        {
            await _authService.OnAccountAuth(response, token);
            await Task.CompletedTask;
        }
    }
}



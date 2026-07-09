using Assets.Scripts.Auth.Services;
using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Auth.ResponseHandlers
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



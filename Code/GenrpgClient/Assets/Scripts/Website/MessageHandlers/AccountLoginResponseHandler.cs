using Assets.Scripts.Login.Messages.Core;
using System.Threading;
using UnityEngine;
using Genrpg.Shared.Accounts.WebApi.Login;
using System.Threading.Tasks;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class AccountLoginResponseHandler : BaseClientWebResponseHandler<AccountLoginResponse>
    {
        private IClientAuthService _authService;
        protected override void InnerProcess(AccountLoginResponse response, CancellationToken token)
        {
            _awaitableService.ForgetAwaitable(InnerProcessAsync(response, token));
        }

        private async Awaitable InnerProcessAsync(AccountLoginResponse response, CancellationToken token)
        {

            await _authService.OnAccountLogin(response, token);
            await Task.CompletedTask;
        }
    }
}


using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedPlatform.Accounts.WebApi.Login;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class AccountLoginResponseHandler : BaseClientWebResponseHandler<AccountLoginResponse>
    {
        private IClientAuthService _authService = null;
        protected override async Awaitable InnerProcess(AccountLoginResponse response, CancellationToken token)
        {

            await _authService.OnAccountLogin(response, token);
            await Task.CompletedTask;
        }
    }
}



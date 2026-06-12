
using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class AccountAuthResponseHandler : BaseClientWebResponseHandler<AccountAuthResponse>
    {
        private IClientAuthService _authService = null;
        protected override async Awaitable InnerProcess(AccountAuthResponse response, CancellationToken token)
        {

            await _authService.OnAccountLogin(response, token);
            await Task.CompletedTask;
        }
    }
}



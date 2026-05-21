using OxDb.PlatformServer.AccountAuthRequests.Constants;
using OxDb.PlatformServer.Accounts.PlayerData;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedPlatform.Accounts.WebApi.Login;

namespace OxDb.PlatformServer.AccountAuthRequests.RequestHandlers
{
    public class AccountLoginRequestHandler : BaseAccountAuthRequestHandler<AccountLoginRequest>
    {
        protected override async Task HandleRequestInternal(IWebContext context, AccountLoginRequest request, CancellationToken token)
        {
            Account account = null;
            if (!string.IsNullOrEmpty(request.AccountId))
            {
                account = await _repoService.Load<Account>(request.AccountId);
                if (account == null)
                {
                    context.ShowError("Invalid login.");
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(request.Email))
            {
                account = (await _repoService.Search<Account>(x => x.LowerEmail == request.Email.ToLower())).FirstOrDefault();

                if (account == null)
                {
                    context.ShowError("Invalid login.");
                    return;
                }
            }
            else
            {
                context.ShowError("Invalid login.");
                return;
            }

            EAuthResponse response = ExistingPasswordIsOk(account, request);

            if (response == EAuthResponse.Failure)
            {
                context.ShowError("Invalid login.");
                return;
            }

            await AfterAuthSuccess(context, account, request, response);

            await Task.CompletedTask;
        }
    }
}



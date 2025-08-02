using Genrpg.RequestServer.AuthRequests.Constants;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Accounts.WebApi.Login;

namespace Genrpg.RequestServer.AuthRequests.AccountAuthRequestHandlers
{
    public class AccountLoginRequestHandler : BaseAccountAuthRequestHandler<AccountLoginRequest>
    {
        protected override async Task HandleRequestInternal(WebContext context, AccountLoginRequest request, CancellationToken token)
        {
            Account account = null;
            if (!string.IsNullOrEmpty(request.UserId))
            {
                account = await _repoService.Load<Account>(request.UserId);
                if (account == null)
                {
                    context.ShowError("That account doesn't exist.");
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(request.Email))
            {
                account = (await _repoService.Search<Account>(x => x.LowerEmail == request.Email.ToLower())).FirstOrDefault();

                if (account == null)
                {
                    context.ShowError("That email isn't linked to an account.");
                    return;
                }
            }
            else
            {
                context.ShowError("You must specify a UserId or an email to log in.");
                return;
            }

            EAuthResponse response = ExistingPasswordIsOk(account, request);

            if (response == EAuthResponse.Failure)
            {
                context.ShowError("Login information is incorrect.");
                return;
            }

            await AfterAuthSuccess(context, account, request, response);

            await Task.CompletedTask;
        }
    }
}

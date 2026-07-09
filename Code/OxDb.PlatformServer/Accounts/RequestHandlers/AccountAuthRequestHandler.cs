using Microsoft.Extensions.DependencyInjection;
using OxDb.PlatformServer.Accounts.AuthHelpers;
using OxDb.PlatformServer.Accounts.Entities;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedPlatform.Accounts.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;

namespace OxDb.PlatformServer.Accounts.RequestHandlers
{
    public class AccountAuthRequestHandler : BaseAccountAuthRequestHandler<AccountAuthRequest>
    {

        private SetupDictionaryContainer<EAuthTypes, IAccountAuthHelper> _authHelpers = new SetupDictionaryContainer<EAuthTypes, IAccountAuthHelper>();

        protected override async Task HandleRequestInternal(IWebContext context, AccountAuthRequest request, CancellationToken token)
        {

            if (string.IsNullOrEmpty(request.DeviceId))
            {
                context.AddResponse(new AccountAuthResponse("Unknown device"));         
                return;
            }

            if (!_authHelpers.TryGetValue(request.AuthType, out IAccountAuthHelper helper))
            {
                context.AddResponse(new AccountAuthResponse("Unknown auth type: " + request.AuthType.ToString()));
                return;
            }

            AccountAuthResult result = await helper.CheckAuthAsync(context, request, token);

            if (!result.Success)
            {
                context.AddResponse(new AccountAuthResponse(result.ErrorMessage));
                _logService.Error("AuthFailure: " + request.AuthType.ToString() + ": " +  result.ErrorMessage);
                return;
            }

            if (result.CurrentAccount == null)
            {
                context.AddResponse(new AccountAuthResponse("Internal Auth Failure, Please try again later."));
                return;
            }

            string installSource = request.InstallSource;

            await AfterAuthSuccess(context, result, request);

            await Task.CompletedTask;
        }

    }
}

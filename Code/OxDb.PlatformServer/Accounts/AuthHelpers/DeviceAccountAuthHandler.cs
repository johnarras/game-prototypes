using OxDb.PlatformServer.Accounts.Entities;
using OxDb.PlatformServer.Accounts.PlayerData;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedPlatform.Accounts.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;

namespace OxDb.PlatformServer.Accounts.AuthHelpers
{
    public class DeviceAccountAuthHandler : BaseAccountAuthHelper
    {
        public override EAuthTypes HelperKey => EAuthTypes.Device;

        public override void Init()
        {
            
        }

        public override async Task<AccountAuthResult> CheckAuthAsync(IWebContext context, IAccountAuthRequest request, CancellationToken token)
        {
            AccountAuthResult result = CreateAuthResult();

            if (string.IsNullOrEmpty(request.AccountId))
            {
                result.ErrorMessage = "Unknown account Id for device login.";
                return result;
            }

            Account account = await _repoService.Load<Account>(request.AccountId);

            if (account == null)
            {
                result.ErrorMessage = "Missing account Id for device login.";
                return result;
            }

            DeviceAuthStatus record = account.AuthRecords.FirstOrDefault(x => x.DeviceId == request.UserIdentity)!;

            if (record == null)
            {
                result.ErrorMessage = "Invalid device login.";
                return result;
            }

            string hashedToken = _cryptoService.GetPasswordHash(record.TokenSalt, request.UserSecret);

            if (record.TokenHash != hashedToken)
            {
                result.ErrorMessage = "Invalid device login.";
                return result;
            }

            result.CurrentAccount = account;
            result.Success = true;

            return result;
        }
    }
}

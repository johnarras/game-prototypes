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

        public override async Task<AccountAuthResult> CheckAuthAsync(IWebContext context, IAccountAuthRequest request)
        {
            AccountAuthResult result = CreateAuthResult();

            if (string.IsNullOrEmpty(request.AccountId))
            {
                result.ErrorMessage = "Unknown account Id for device login.";
                return result;
            }

            Account acc = await _repoService.Load<Account>(request.AccountId);

            if (acc == null)
            {
                result.ErrorMessage = "Missing account Id for device login.";
                return result;
            }

            AuthRecord record = acc.AuthRecords.FirstOrDefault(x => x.DeviceId == request.UserIdentity)!;

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

            result.CurrentAccount = acc;
            result.Success = true;

            return result;
        }
    }
}

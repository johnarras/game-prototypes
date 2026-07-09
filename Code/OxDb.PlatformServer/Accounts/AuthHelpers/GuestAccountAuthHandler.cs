using OxDb.PlatformServer.Accounts.Entities;
using OxDb.PlatformServer.Accounts.PlayerData;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedPlatform.Accounts.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;
using System.Security.AccessControl;

namespace OxDb.PlatformServer.Accounts.AuthHelpers
{
    public class GuestAccountAuthHandler : BaseAccountAuthHelper
    {
        public override EAuthTypes HelperKey => EAuthTypes.Guest;

        public override void Init()
        {

        }

        public override async Task<AccountAuthResult> CheckAuthAsync(IWebContext context, IAccountAuthRequest request, CancellationToken token)
        {
            AccountAuthResult result = CreateAuthResult();

            if (string.IsNullOrEmpty(request.UserIdentity))
            {
                result.ErrorMessage = "Missing guest login id.";
                return result;
            }

            Account account = null;

            // First case, see if this device token is in use already.
            if (string.IsNullOrEmpty(request.AccountId))
            {
                account = (await _repoService.Search<Account>(x => x.GuestDeviceId == request.UserIdentity)).FirstOrDefault()!;

            }
            else
            {
                account = await _repoService.Load<Account>(request.AccountId);
            }

            // If account exists, now check secret that was sent.
            if (account != null)
            {
                // Regardless of whether this was an old account or a new one, we now check the sent password vs the account data.
                // See if this matches the 
                string newPasswordHash = _cryptoService.GetPasswordHash(account.GuestSecretSalt, request.UserSecret);

                if (newPasswordHash != account.GuestSecretHash)
                {
                    result.ErrorMessage = "The guest secret does not match.";
                    return result;
                }
            }
            else // Make a new account and set the guest device id and the guest secret and send it to client.
            {
                account = await _accountService.CreateNewAccount(request);

                string guestSecret = HashUtils.NewGuid();

                string secretSalt = _cryptoService.GetRandomByteString(16);

                account.GuestDeviceId = request.UserIdentity;
                account.GuestSecretSalt = secretSalt;
                account.GuestSecretHash = _cryptoService.GetPasswordHash(secretSalt, guestSecret);


                result.OneTimeGuestAccountId = account.Id;
                result.OneTimeGuestSecret = guestSecret;
            }

            result.CurrentAccount = account;
            result.Success = true;

            return result;
        }
    }
}

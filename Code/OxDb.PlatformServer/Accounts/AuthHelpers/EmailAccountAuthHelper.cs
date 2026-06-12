using OxDb.PlatformServer.Accounts.Constants;
using OxDb.PlatformServer.Accounts.Entities;
using OxDb.PlatformServer.Accounts.PlayerData;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedPlatform.Accounts.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;

namespace OxDb.PlatformServer.Accounts.AuthHelpers
{
    public class EmailAccountAuthHelper : BaseAccountAuthHelper
    {
        public override EAuthTypes HelperKey => EAuthTypes.Email;

        public override async Task<AccountAuthResult> CheckAuthAsync(IWebContext context, IAccountAuthRequest request)
        {
            AccountAuthResult result = CreateAuthResult();

            Account account = null!;

            if (string.IsNullOrEmpty(request.UserIdentity) || string.IsNullOrEmpty(request.UserSecret))
            {
                result.ErrorMessage = "Email and password cannot be blank.";
                return result;
            }

            if (!string.IsNullOrEmpty(request.AccountId))
            {
                account = await _repoService.Load<Account>(request.AccountId);

                if (account == null)
                {
                    result.ErrorMessage = "No account with that Id exists.";
                    return result;
                }
            }
            else // Need to make account. Check for existing email.
            {
                // First see if there's an account with this email.

                account = (await _repoService.Search<Account>(x => x.LowerEmail == request.UserIdentity.ToLower())).FirstOrDefault()!;

                if (account == null)
                {
                    if (!await EmailIsOk(context, result, request.UserIdentity))
                    {
                        return result;
                    }

                    if (!await NewPasswordIsOk(context, result, request.UserSecret))
                    {
                        return result;
                    }
                    string passwordSalt = _cryptoService.GetRandomByteString(16);
                    string passwordHash = _cryptoService.GetPasswordHash(passwordSalt, request.UserSecret);
                    account = await _accountService.CreateNewAccount(request);

                    account.PasswordHash = passwordHash;
                    account.PasswordSalt = passwordSalt;
                    account.Email = request.UserIdentity;
                    account.LowerEmail = request.UserIdentity.ToLower();
                }
            }

            // Regardless of whether this was an old account or a new one, we now check the sent password vs the account data.
            string newPasswordHash = _cryptoService.GetPasswordHash(account.PasswordSalt, request.UserSecret);

            if (newPasswordHash != account.PasswordHash)
            {
                result.ErrorMessage = "Email or Password do not match.";
                return result;
            }

            result.Success = true;
            result.CurrentAccount = account;

            return result;
        }


        protected async Task<bool> EmailIsOk(IWebContext context, AccountAuthResult result, string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                result.ErrorMessage = "Email cannot be blank.";
                return false;
            }

            int atIndex = email.IndexOf("@");
            int lastDotIndex = email.LastIndexOf(".");
            if (atIndex < 1 || lastDotIndex < 2 ||
                atIndex >= lastDotIndex ||
                lastDotIndex < atIndex + 2 ||
                lastDotIndex >= email.Length - 2)
            {
                result.ErrorMessage = "This doesn't look like a valid email.";
                return false;
            }

            await Task.CompletedTask;
            return true;
        }

        protected async Task<bool> NewPasswordIsOk(IWebContext context, AccountAuthResult result, string password)
        {
            string passwordError = $"Password must be at least {AccountConstants.MinPasswordLength} characters long";
            if (string.IsNullOrEmpty(password) ||
                password.Length < AccountConstants.MinPasswordLength)
            {
                result.ErrorMessage = passwordError;
                return false;
            }
            await Task.CompletedTask;
            return true;
        }
    }
}

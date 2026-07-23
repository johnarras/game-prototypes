using Google.Apis.Auth;
using OxDb.PlatformServer.Accounts.Entities;
using OxDb.PlatformServer.Accounts.PlayerData;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.WebRequests.Services;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedPlatform.Accounts.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;

namespace OxDb.PlatformServer.Accounts.AuthHelpers
{
    /// <summary>
    /// Strictly typed schema mapping for Google's raw JSON response
    /// </summary>
    public class GoogleTokenPayload
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }
        public string id_token { get; set; }
    }


    public class GooglePlayAccountAuthHandler : BaseAccountAuthHelper
    {

        const string AuthEndpoint = "https://oauth2.googleapis.com/token";

        public override EAuthTypes HelperKey => EAuthTypes.GooglePlay;


        private string _serverSecret = null;
        private string _webServerClientId = null;
            
        public override void Init()
        {
            _webServerClientId = _serverConfig.GetConfigVal(AppConfigKeys.GooglePlayServerClientId);
            _serverSecret = _serverConfig.GetConfigVal(AppConfigKeys.GooglePlayServerSecret);
        }

        public override async Task<AccountAuthResult> CheckAuthAsync(IWebContext context, IAccountAuthRequest request, CancellationToken token)
        {
            AccountAuthResult result = CreateAuthResult();

            
            if (string.IsNullOrEmpty(request.UserIdentity))
            {
                result.ErrorMessage = "No Auth Code sent to validate with GooglePlay.";
                return result;
            }


            string authCode = request.UserIdentity;
            Dictionary<string, string> formParameters = new Dictionary<string, string>
            {
                { "code", authCode },
                { "client_id", _webServerClientId },
                { "client_secret", _serverSecret },
                { "redirect_uri", "" }, // Must be passed empty
                { "grant_type", "authorization_code" }
            };
            FormUrlEncodedContent requestBody = new FormUrlEncodedContent(formParameters);

            bool didSucceed = false;


            WebRequestOptions opts = new WebRequestOptions()
            {
                Method = HttpMethodType.Post,
                FormBody = formParameters,
                ContentType = HttpContentType.FormUrlEncoded,
            };

            ResponseEnvelope<GoogleTokenPayload> tokenResponse = await _webRequestService.SendAsync<GoogleTokenPayload>(AuthEndpoint, opts, token);

            if (tokenResponse == null || 
                !tokenResponse.Success ||
                string.IsNullOrEmpty(tokenResponse.Response.id_token))
            {
                result.ErrorMessage = "Google Response did not contain an id_token.";
                return result;
            }

            GoogleJsonWebSignature.ValidationSettings validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                // Ensures the token was actually minted specifically for your client backend app
                Audience = new List<string> { _webServerClientId }
            };

            GoogleJsonWebSignature.Payload verifiedPayload = null;
            string googlePlayUserId = null;

            string googlePlayEmail = null;
            try

            {

               verifiedPayload = await GoogleJsonWebSignature.ValidateAsync(
                        tokenResponse.Response.id_token,
                        validationSettings
                    );


                googlePlayUserId = verifiedPayload.Subject;
                googlePlayEmail = verifiedPayload.Email;
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "GooglePlayAuthValidation");
            }

            if (string.IsNullOrEmpty(googlePlayUserId))
            {
                result.ErrorMessage = "Failed to Validate GooglePlay Auth Response";
                return result;
            }

            Account account = null;

            if (!string.IsNullOrEmpty(request.AccountId))
            {
                account = await _repoService.Load<Account>(request.AccountId);

                if (account == null)
                {
                    result.ErrorMessage = "Invalid AccountId sent for auth.";
                    return result;
                }
                if (!string.IsNullOrEmpty(account.GooglePlayUserId) && account.GooglePlayUserId != googlePlayUserId)
                {
                    result.ErrorMessage = "This Account is already linked to another GooglePlay account.";
                    return result;
                }                        

                // If we get to this point, the account was sent up and it matches the account used for the google play
                // auth, so we can just keep using it.
            }

            // Did not send up an AccountId with the request, so try to find the account for this GooglePlay user id.
            if (account == null)
            {
                account = (await _repoService.Search<Account>(x => x.GooglePlayUserId == googlePlayUserId)).FirstOrDefault()!;

                // If an account has this GooglePlay id and it's not the same 
                if (account != null)
                {
                    if (!string.IsNullOrEmpty(request.AccountId) && account.Id != request.AccountId)
                    {
                        result.ErrorMessage = "This GooglePlay User is already being used by another Account.";
                        return result;
                    }

                    // Otherwise, no AccountId was sent up, so this is the account we want for the GooglePlay user id.
                }
                else // No account for this GooglePlay user id, so let's make one.
                {
                    account = await _accountService.CreateNewAccount(request);
                }

                // At this point we have a valid Account that was either created, or matches the GooglePlay user id sent
                // up, and this GooglePlay user id is not being used by any other accounts, and the player 
                // isn't logged in to a different account that's already linked to GooglePlay. 
                // Hopefully this is all of the cases.
            }

            account.GooglePlayUserId = googlePlayUserId;
            if (!string.IsNullOrEmpty(googlePlayEmail))
            {
                account.GooglePlayEmail = googlePlayEmail;
                account.LowerGoogleEmail = googlePlayEmail.ToLower();
            }

            _logService.Info("GooglePlay Auth Success to AccountId: " + account.Id);
            result.CurrentAccount = account;
            result.Success = true;

            return result;
        }
    }
}

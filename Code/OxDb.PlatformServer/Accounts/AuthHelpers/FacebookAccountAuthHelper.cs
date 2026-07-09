using Google.Apis.Auth;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Azure.Cosmos.Linq;
using OxDb.PlatformServer.Accounts.Entities;
using OxDb.PlatformServer.Accounts.PlayerData;
using OxDb.ServerCore.WebRequests.Services;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedPlatform.Accounts.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace OxDb.PlatformServer.Accounts.AuthHelpers
{

    // This matches the outer JSON object: { "data": { ... } }
    public class FacebookTokenPayload
    {
        [JsonPropertyName("data")]
        public FacebookTokenData Data { get; set; }
    }

    // This matches everything inside the "data" block
    public class FacebookTokenData
    {
        [JsonPropertyName("app_id")]
        public string AppId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("application")]
        public string Application { get; set; }

        [JsonPropertyName("expires_at")]
        public long ExpiresAt { get; set; }

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }
    }

    public class FacebookUserProfile
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }
    }


    public class FacebookAccountAuthHelper : BaseAccountAuthHelper
    {
        // Form https://graph...?input_token={facebookAccessTokenFromClient}&access_token=appId|appSecret;
        // So we create the prefix just before the client access token, and then create the suffix from
        // the facebookAppId and the facebookAppSecret, then it's 
        // Prefix + clientFacebookAccessToken + Suffix

        const string AuthEndpointPrefix = "https://graph.facebook.com/debug_token?input_token=";


        public override EAuthTypes HelperKey => EAuthTypes.Facebook;


        private string _facebookAppId = null;
        private string _facebookAppSecret = null;

        private string _endpointSuffix = null;

        public override void Init()
        {
            _facebookAppId = _serverConfig.GetConfigVal(AppConfigKeys.FacebookAppId);
            _facebookAppSecret = _serverConfig.GetConfigVal(AppConfigKeys.FacebookAppSecret);

            _endpointSuffix = "&access_token=" + _facebookAppId + "|" + _facebookAppSecret;
        }

        public override async Task<AccountAuthResult> CheckAuthAsync(IWebContext context, IAccountAuthRequest request, CancellationToken token)
        {
            AccountAuthResult result = CreateAuthResult();


            if (string.IsNullOrEmpty(request.UserIdentity))
            {
                result.ErrorMessage = "Facebook User Id not sent to auth with Facebook.";
                return result;
            }

            if (string.IsNullOrEmpty(request.UserSecret))
            {
                result.ErrorMessage = "No Access Token was sent to auth with Facebook.";
                return result;
            }


            string url = AuthEndpointPrefix + request.UserSecret + _endpointSuffix;


            string userEmail = null;

            try
            {

                ResponseEnvelope<FacebookTokenPayload> payloadEnvelope = await _webRequestService.GetAsync<FacebookTokenPayload>(url);

                if (!payloadEnvelope.Success)
                {
                    result.ErrorMessage = payloadEnvelope.ErrorMessage;
                    return result;
                }

                FacebookTokenPayload payload = payloadEnvelope.Response;

                if (payload?.Data == null)
                {
                    result.ErrorMessage = "Facebook Payload had no data.";
                    return result;
                }

                if (!payload.Data.IsValid)
                {
                    result.ErrorMessage = "Facebook payload is not valid.";
                    return result;
                }

                if (payload.Data.AppId != _facebookAppId)
                {
                    result.ErrorMessage = "This is the wrong Facebook App.";
                    return result;
                }

                if (payload.Data.UserId != request.UserIdentity)
                {
                    result.ErrorMessage = "This user Token is for a different userId.";
                    return result;
                }

                if (payload.Data.ExpiresAt > 0)
                {
                    if (payload.Data.ExpiresAt <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    {
                        result.ErrorMessage = "The Facebook Data Payload has expired.";
                        return result;
                    }
                }

                string profileUrl = $"https://graph.facebook.com/me?fields=id,name,email&access_token={request.UserSecret}";

                // Use your general GET method to fetch the profile
                ResponseEnvelope<FacebookUserProfile> profileEnvelope = await _webRequestService.GetAsync<FacebookUserProfile>(profileUrl);

                if (!profileEnvelope.Success)
                {
                    _logService.Error("Failed to get user profile for user " + request.UserIdentity);
                }

                FacebookUserProfile profile = profileEnvelope.Response;

                if (profile != null && !string.IsNullOrEmpty(profile.Email))
                {
                    userEmail = profile.Email;  
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "FacebookAuth");
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
                if (!string.IsNullOrEmpty(account.FacebookUserId) && account.FacebookUserId != request.UserIdentity)
                {
                    result.ErrorMessage = "This Account is already linked to another Facebook account.";
                    return result;
                }

                // If we get to this point, the account was sent up and it matches the account used for the google play
                // auth, so we can just keep using it.
            }

            // Did not send up an AccountId with the request, so try to find the account for this GooglePlay user id.
            if (account == null)
            {
                account = (await _repoService.Search<Account>(x => x.FacebookUserId == request.UserIdentity)).FirstOrDefault()!;

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

            account.FacebookUserId = request.UserIdentity;
            if (!string.IsNullOrEmpty(userEmail))
            {
                account.FacebookEmail = userEmail;
                account.LowerFacebookEmail = userEmail.ToLower();
            }

            _logService.Info("Facebook Auth Success to AccountId: " + account.Id);
            result.CurrentAccount = account;
            result.Success = true;

            return result;
        }
    }
}

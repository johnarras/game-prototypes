
using OxDb.SharedCore.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Auth.Services
{
    public interface IFacebookAuthService : IInitializable
    {
        void BeginAuth(CancellationToken token);
    }

    public sealed class FacebookAuthService : IFacebookAuthService
    {
        //private readonly ILogService _logService = null;
        //private readonly IClientAuthService _clientAuthService = null;

        private CancellationToken _token;

        public void BeginAuth(CancellationToken token)
        {
            //if (!FB.IsInitialized)
            //{
            //    _logService.Error("[Auth] Cannot log in; Facebook SDK is not initialized yet.");
            //    return;
            //}

            //List<string> permissions = new List<string> { "public_profile", "email" };
            //FB.LogInWithReadPermissions(permissions, OnFacebookLoginResult);

        }

        public async Task Initialize(CancellationToken token)
        {
            _token = token;

            //if (!FB.IsInitialized)
            //{
            //    // Initialize the SDK asynchronously
            //    FB.Init(OnFacebookInitialized, OnHideUnityWindow);
            //}
            //else
            //{
            //    FB.ActivateApp();
            //}

            await Task.CompletedTask;
        }

        //private void OnFacebookInitialized()
        //{
        //    if (FB.IsInitialized)
        //    {
        //        FB.ActivateApp();
        //        _logService.Info("[Auth] Facebook SDK successfully initialized.");
        //    }
        //    else
        //    {
        //        _logService.Error("[Auth] Failed to initialize the Facebook SDK.");
        //    }
        //}

        //private void OnHideUnityWindow(bool isGameShown)
        //{
        //    Time.timeScale = isGameShown ? 1.0f : 0.0f;
        //}


        //private void OnFacebookLoginResult(ILoginResult result)
        //{
        //    if (result == null)
        //    {
        //        _logService.Error("[Auth] Facebook login returned a null result.");
        //        return;
        //    }

        //    if (!string.IsNullOrEmpty(result.Error))
        //    {
        //        _logService.Error($"[Auth] Facebook login error: {result.Error}");
        //        return;
        //    }

        //    if (result.Cancelled)
        //    {
        //        _logService.Info("[Auth] Player cancelled the Facebook login process.");
        //        return;
        //    }

        //    if (FB.IsLoggedIn && AccessToken.CurrentAccessToken != null)
        //    {
        //        string facebookAccessToken = AccessToken.CurrentAccessToken.TokenString;
        //        string facebookUserId = AccessToken.CurrentAccessToken.UserId;

        //        _logService.Info($"[Auth] Facebook token acquired successfully for User: {facebookUserId}");

        //        // Dispatch the token and verify completion
        //        SendTokenToBackend(facebookUserId, facebookAccessToken);
        //    }
        //    else
        //    {
        //        _logService.Error("[Auth] Login claim succeeded but AccessToken is missing.");
        //    }
        //}

        //private void SendTokenToBackend(string facebookUserId, string facebookAccessToken)
        //{
        //    AccountAuthRequest request = new AccountAuthRequest()
        //    {
        //        AuthType = EAuthTypes.Facebook,
        //        UserIdentity = facebookUserId,
        //        UserSecret = facebookAccessToken,
        //    };

        //    _logService.Info("Sending FB auth request to server");
        //    _clientAuthService.SendAccountAuthRequest(request, true, _token);
        //}
    }
}
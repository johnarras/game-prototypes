#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedPlatform.Accounts.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Auth.Services
{
    public interface IGooglePlayAuthService : IInitializable
    {
        void BeginAuth(CancellationToken token);
    }

    public class GooglePlayAuthService : IGooglePlayAuthService
    {

        private ILogService _logService = null;
        private IClientAuthService _authService = null;


        public async Task Initialize(CancellationToken token)
        {
            try
            {

                PlayGamesPlatform.DebugLogEnabled = true;
                PlayGamesPlatform.Activate();
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "Google Play Init");
            }
            await Task.CompletedTask;
        }


        public void BeginAuth(CancellationToken token)
        {

            try
            {
                // 2. Authenticate using the modern interactivity enum
                PlayGamesPlatform.Instance.Authenticate((SignInStatus status) =>
                {
                    if (status == SignInStatus.Success)
                    {
                        _logService.Info("Google Play Games login successful. Retrieving auth code...");
                        FetchServerAuthCode(token);
                    }
                    else
                    {
                        _logService.Error($"Google Play Sign-In Failed with status code: {status}");
                    }
                });
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "GooglePlay.Auth");
            }
        }

        private void FetchServerAuthCode(CancellationToken token)
        {
            // true requests a refresh token alongside the code, which is ideal for server validation pipelines
            PlayGamesPlatform.Instance.RequestServerSideAccess(true, (string authCode) =>
            {
                if (!string.IsNullOrEmpty(authCode))
                {
                    _logService.Info($"Successfully retrieved Server Auth Code: {authCode}");
                    SendCodeToServer(authCode, token);
                }
                else
                {
                    _logService.Error("Server side access returned an empty or null authorization token. " +
                                   "Check your Cloud Console OAuth consent configurations.");
                }
            });
        }

        private void SendCodeToServer(string authCode, CancellationToken token)
        {
            AccountAuthRequest request = new AccountAuthRequest()
            {
                AuthType = EAuthTypes.GooglePlay,
                UserIdentity = authCode,
            };

            _authService.SendAccountAuthRequest(request, true, token);
        }
    }
}
#endif
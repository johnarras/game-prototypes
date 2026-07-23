
using OxDb.Client.Awaitables;
using OxDb.Client.ClientEvents.UI;
using OxDb.Client.Core.Interfaces;
using OxDb.Client.Networking.Services;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedCore.Website.Responses.Errors;
using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.GameAuth.WebApi.Auth;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.UI.Constants;
using OxDb.SharedGame.Versions.Settings;
using OxDb.SharedPlatform.Accounts.Constants;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace OxDb.Client.Auth.Services
{
    public interface IClientAuthService : IInitializable, IClientResetCleanup
    {
        ValueTask StartAuth(CancellationToken token);
        ValueTask Logout();
        void SendAccountAuthRequest(AccountAuthRequest request, bool addAccountId, CancellationToken token);
        ValueTask UpdateLocalUserDataFromAuthResponse(AccountAuthResponse response);
        ValueTask StartNoUser(CancellationToken token);
        ValueTask OnAccountAuth(AccountAuthResponse response, CancellationToken token);
        ValueTask StartGuestLogin(CancellationToken token);
        ValueTask<string> GetCurrentAccountId();

    }

    public class ClientAuthService : IClientAuthService
    {
        private const string LocalUserFilename = "LocalUser";

        private IClientWebRequestService _clientWebService = null;
        private IZoneGenService _zoneGenService = null;
        private IRepositoryService _repoService = null;
        private ILogService _logService = null;
        protected IGameData _gameData = null;
        protected IPlayerManager _playerManager = null;
        protected IMapProvider _mapProvider = null;
        protected IClientGameState _gs = null;
        private IClientConfigContainer _config = null;
        private IClientCryptoService _clientCryptoService = null;
        private IClientAppService _clientAppService = null;
        protected IAwaitableService _awaitableService = null;
        private ITextSerializer _serializer = null;
        private IDispatcher _dispatcher = null;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async ValueTask StartAuth(CancellationToken token)
        {
            LocalUserData localData = await LoadLocalUserData();

            string accountId = localData.AccountId;
            string userIdentity = localData.UserId;
            string userSecret = _clientCryptoService.SafeDecryptString(localData.LoginToken);

            if (!string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(userIdentity) && !string.IsNullOrEmpty(userSecret))
            {
                _logService.Info("Attempting Device Login: " + accountId + " -- " + userIdentity + " " + userSecret);
                AccountAuthRequest deviceAuthRequest = new AccountAuthRequest()
                {
                    AuthType = EAuthTypes.Device,
                    AccountId = accountId,
                    UserIdentity = _clientCryptoService.GetDeviceId(),
                    UserSecret = userSecret,
                };
                SendAccountAuthRequest(deviceAuthRequest, false, token);
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.Loading, true));
                return;
            }

            // Otherwise we either had no local login or we had no valid online login, and in this case
            // show the login screen.      
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.GetMainAuthScreen(), true));
            _dispatcher.Dispatch(new CloseScreen(ScreenNames.Loading));

        }

        protected LocalUserData _currData = null;
        protected async ValueTask<LocalUserData> LoadLocalUserData()
        {
            if (_currData != null)
            {
                return _currData;
            }

            LocalUserData localData = await _repoService.Load<LocalUserData>(LocalUserFilename);
            if (localData == null)
            {
                localData = new LocalUserData()
                {
                    Id = LocalUserFilename,
                };
                await SaveLocalUserData(localData);
            }
            _currData = localData;
            return localData;
        }

        protected async ValueTask SaveLocalUserData(LocalUserData localData)
        {
            localData.Id = LocalUserFilename;
            await _repoService.Save(localData);
        }

        public async ValueTask Logout()
        {
            _logService.Info("Logging out");
            _zoneGenService.ExitMMOMap();
            _gs.GameUserId = null;
            _gs.SessionState = new StubSessionState();
            _dispatcher.Dispatch(new CloseAllScreens());
            _dispatcher.Dispatch(new CloseScreen(ScreenNames.HUD));
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.GetMainAuthScreen()));

            LocalUserData userData = await LoadLocalUserData();

            if (userData.LastAuthType == EAuthTypes.Guest)
            {
                return;
            }

            userData.ClearData();
            await SaveLocalUserData(userData);

        }
        public async ValueTask UpdateLocalUserDataFromAuthResponse(AccountAuthResponse response)
        {
            LocalUserData localUserData = await LoadLocalUserData();

            localUserData.AccountId = response.AccountId;
            localUserData.UserId = response.ProductUserId;
            localUserData.LoginToken = _clientCryptoService.EncryptString(response.LoginToken);
            localUserData.ValidAuthTypes = response.ValidAuthTypes;

            if (response.LastAuthType != EAuthTypes.Device)
            {
                localUserData.LastAuthType = response.LastAuthType;
            }
            if (response.LastAuthType == EAuthTypes.Guest &&
                string.IsNullOrEmpty(localUserData.GuestSecret)
                && !string.IsNullOrEmpty(response.OneTimeGuestSecret)
                && string.IsNullOrEmpty(localUserData.GuestAccountId)
                && !string.IsNullOrEmpty(response.OneTimeGuestAccountId))
            {
                localUserData.GuestAccountId = response.OneTimeGuestAccountId;
                localUserData.GuestSecret = response.OneTimeGuestSecret;
            }

            await SaveLocalUserData(localUserData);
        }

        public void SendAccountAuthRequest(AccountAuthRequest request, bool addAccountId, CancellationToken token)
        {
            request.DeviceId = _clientCryptoService.GetDeviceId();
            request.ProductId = _config.Config.ProductId;
            request.ClientVersion = _clientAppService.Version;

            if (addAccountId && string.IsNullOrEmpty(request.AccountId))
            {
                request.AccountId = GetCurrentAccountId().Result;
            }

            _clientWebService.SendMainServerRequest(request, token);
        }

        public async ValueTask StartNoUser(CancellationToken token)
        {
            GameAuthResponse result = new GameAuthResponse() { GameUserId = "Local", FullToken = "Local", GameSessionId = "Local" };

            WebServerResponseSet resultSet = new WebServerResponseSet() { Responses = new List<IWebResponse>() { result } };

            string txt = _serializer.SerializeToString(resultSet);
            await _clientWebService.HandleResponses(txt, null, token);
            await Task.CompletedTask;
        }

        public async Task OnReset(CancellationToken token)
        {
            _zoneGenService.ExitMMOMap();
            await Task.CompletedTask;
        }

        public async ValueTask OnAccountAuth(AccountAuthResponse response, CancellationToken token)
        {
            _logService.Info("AccountAuthResponse: " + response.AccountId + " -- " + response.LoginToken + " " + response.ProductUserId + " "
                + response.LastAuthType);
            if (response.Success &&
                !string.IsNullOrEmpty(response.AccountId) &&
                !string.IsNullOrEmpty(response.LoginToken) &&
                (!string.IsNullOrEmpty(response.ProductUserId) ||
                response.ProductId == _config.Config.ProductId))
            {
                await UpdateLocalUserDataFromAuthResponse(response);
            }
            else
            {
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    _dispatcher.Dispatch(new ErrorResponse() { Error = response.ErrorMessage });
                }

                LocalUserData localUserData = await LoadLocalUserData();

                localUserData.ClearData();
                await SaveLocalUserData(localUserData);

                _dispatcher.Dispatch(new CloseScreen(ScreenNames.Loading));
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.GetMainAuthScreen()));
                return;
            }

            GameAuthRequest request = new GameAuthRequest()
            {
                AccountId = response.AccountId,
                AccountSessionId = response.AccountSessionId,
                GameUserId = response.ProductUserId,
                ClientVersion = _clientAppService.Version,
                ClientPlatformName = _clientAppService.GetPlatformName(),
                ClientGameDataSaveTime = _gameData.Get<VersionSettings>(null).SaveTime,
                ProductName = _gs.GameMode.ToString(),
                DisplayName = response.DisplayName,
                DataBits = response.DataBits,
                ProductId = response.ProductId,
            };

            _clientWebService.SendMainServerRequest(request, token);

        }

        public async ValueTask StartGuestLogin(CancellationToken token)
        {
            LocalUserData localData = await LoadLocalUserData();

            string decryptedGuestSecret = _clientCryptoService.SafeDecryptString(localData.GuestSecret);

            AccountAuthRequest request = new AccountAuthRequest()
            {
                AuthType = EAuthTypes.Guest,
                AccountId = localData.GuestAccountId,
                UserIdentity = _clientCryptoService.GetDeviceId(),
                UserSecret = decryptedGuestSecret,
            };

            SendAccountAuthRequest(request, false, token);

        }

        public async ValueTask<string> GetCurrentAccountId()
        {
            LocalUserData localData = await LoadLocalUserData();
            return localData.AccountId;

        }
    }
}


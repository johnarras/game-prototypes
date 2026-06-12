
using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Core.Interfaces;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Website.Responses.Core;
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


public interface IClientAuthService : IInitializable, IClientResetCleanup
{
    Awaitable StartAuth(CancellationToken token);
    void Logout();
    void SendAccountAuthRequest(AccountAuthRequest request, CancellationToken token);
    Awaitable SaveLocalUserData(string accountId, string gameUserId, string loginToken);
    Awaitable StartNoUser(CancellationToken token);
    Awaitable OnAccountLogin(AccountAuthResponse response, CancellationToken token);
}

public class ClientAuthService : IClientAuthService
{
    private const string LocalUserFilename = "LocalUser";

    private IClientWebService _clientWebService = null;
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

    public async Awaitable StartAuth(CancellationToken token)
    {
        LocalUserData localData = await _repoService.Load<LocalUserData>(LocalUserFilename);

        if (localData != null)
        {
            string accountId = localData.AccountId;
            string userIdentity = localData.UserId;
            string userSecret = _clientCryptoService.DecryptString(localData.LoginToken);

            if (!string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(userIdentity) && !string.IsNullOrEmpty(userSecret))
            {
                AccountAuthRequest deviceAuthRequest = new AccountAuthRequest()
                {
                    AuthType = EAuthTypes.Device,
                    AccountId = accountId,
                    UserIdentity = _clientCryptoService.GetDeviceId(),
                    UserSecret = userSecret,
                };
                SendAccountAuthRequest(deviceAuthRequest, token);
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.Loading, true));
                return;
            }
        }

        // Otherwise we either had no local login or we had no valid online login, and in this case
        // show the login screen.      
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.Login, true));
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.Loading));

    }

    public void Logout()
    {
        _logService.Info("Logging out");
        _zoneGenService.ExitMMOMap();
        _gs.GameUserId = null;
        _gs.SessionState = new StubSessionState();
        _dispatcher.Dispatch(new CloseAllScreens());
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.HUD));
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.Login));
    }
    public async Awaitable SaveLocalUserData(string accountId, string gameUserId, string loginToken)
    {
        LocalUserData localUserData = new LocalUserData()
        {
            Id = LocalUserFilename,
            AccountId = accountId,
            UserId = gameUserId,
            LoginToken = _clientCryptoService.EncryptString(loginToken),
        };

        await _repoService.Save(localUserData);
    }

    public void SendAccountAuthRequest(AccountAuthRequest request, CancellationToken token)
    {
        request.DeviceId = _clientCryptoService.GetDeviceId();
        request.ProductId = _config.Config.ProductId;
        request.ClientVersion = _clientAppService.Version;

        _clientWebService.SendWebRequest(request, token);
    }

    public async Awaitable StartNoUser(CancellationToken token)
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

    public async Awaitable OnAccountLogin(AccountAuthResponse response, CancellationToken token)
    {
        if (!string.IsNullOrEmpty(response.AccountId) &&
            !string.IsNullOrEmpty(response.LoginToken) &&
            (!string.IsNullOrEmpty(response.ProductUserId) ||
            response.ProductId == _config.Config.ProductId))
        {
            await SaveLocalUserData(response.AccountId, response.ProductUserId, response.LoginToken);
        }
        else
        {
            _dispatcher.Dispatch(new CloseScreen(ScreenNames.Loading));
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.Login));
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

        _clientWebService.SendWebRequest(request, token);

    }
}


using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.Accounts.WebApi.Login;
using Genrpg.Shared.Accounts.WebApi.Signup;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameAuth.WebApi.Auth;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Versions.Settings;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public interface IClientAuthService : IInitializable, IClientResetCleanup
{
    void StartAuth(CancellationToken token);
    void Logout();
    void ExitMap();
    Awaitable SendAccountLogin(AccountLoginRequest request, CancellationToken token);
    Awaitable SendSignupRequest(AccountSignupRequest request, CancellationToken token);
    Awaitable SaveLocalUserData(string accountId, string gameUserId, string loginToken);
    Awaitable StartNoUser(CancellationToken token);
    Awaitable OnAccountLogin(AccountLoginResponse response, CancellationToken token);
}

public class ClientAuthService : IClientAuthService
{
    private const string LocalUserFilename = "LocalUser";

    private IClientWebService _clientWebService = null;
    private IRealtimeNetworkService _realtimeNetworkService = null;
    private IMapTerrainManager _mapManager = null;
    private IClientMapObjectManager _objectManager = null;
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

    public void StartAuth(CancellationToken token)
    {
        LocalUserData localData = _repoService.Load<LocalUserData>(LocalUserFilename).Result;

        string accountId = "";
        string userId = "";
        string email = "";
        string password = "";

        if (localData != null)
        {
            try
            {
                accountId = localData.AccountId;
                userId = localData.UserId;
                password = _clientCryptoService.DecryptString(localData.LoginToken);
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "StartLogin");
            }
        }
        if ((!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(accountId)) && !string.IsNullOrEmpty(password))
        {
            AccountLoginRequest LoginRequest = new AccountLoginRequest()
            {
                AccountId = accountId,
                Email = email,
                Password = password,
                DeviceId = _clientCryptoService.GetDeviceId(),
            };

            _awaitableService.ForgetAwaitable(SendAccountLogin(LoginRequest, token));
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.Loading, true));
            return;
        }

        // Otherwise we either had no local login or we had no valid online login, and in this case
        // show the login screen.      
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.Login, true));
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.Loading));

    }

    public void Logout()
    {
        _logService.Info("Logging out");
        ExitMMOMap();
        _gs.GameUserId = null;
        _gs.SessionState = new StubSessionState();
        _dispatcher.Dispatch(new CloseAllScreens());
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.HUD));
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.Login));
    }

    public void ExitMap()
    {
        _logService.Info("Exiting Map");
        ExitMMOMap();
        _dispatcher.Dispatch(new CloseAllScreens());
        _dispatcher.Dispatch(new CloseScreen(ScreenNames.HUD));
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.CharacterSelect));
    }

    private void ExitMMOMap()
    {
        _zoneGenService.CancelMapToken();
        _playerManager.SetUnit(null);
        _realtimeNetworkService.CloseClient();
        _mapManager.Clear();
        _objectManager.Reset();
        _zoneGenService.LoadedMapId = null;
        _mapProvider.SetMap(null);
        _mapProvider.SetSpawns(null);
        _gs.ch = null;

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

    public async Awaitable SendAccountLogin(AccountLoginRequest request, CancellationToken token)
    {
        request.ProductId = _config.Config.ProductId;

        AccountLoginResponse result = await _clientWebService.SendWebRequestAsync<AccountLoginResponse>(request, token);

        if (result == null)
        {
            _logService.Info("Got null result on send of " + request.GetType().Name);
        }
    }

    public async Awaitable StartNoUser(CancellationToken token)
    {
        GameAuthResponse result = new GameAuthResponse() { GameUserId = "Local", SessionToken = "Local" };

        WebServerResponseSet resultSet = new WebServerResponseSet() { Responses = new List<IWebResponse>() { result } };

        string txt = _serializer.SerializeToString(resultSet);
        await _clientWebService.HandleResponses(txt, null, token);
        await Task.CompletedTask;
    }

    public async Awaitable SendSignupRequest(AccountSignupRequest request, CancellationToken token)
    {
        request.ProductId = _config.Config.ProductId;
        _clientWebService.SendWebRequest(request, token);
        await Task.CompletedTask;
    }

    public async Task OnReset(CancellationToken token)
    {
        ExitMMOMap();
        await Task.CompletedTask;
    }

    public async Awaitable OnAccountLogin(AccountLoginResponse response, CancellationToken token)
    {
        if (!string.IsNullOrEmpty(response.AccountId) && !string.IsNullOrEmpty(response.LoginToken) && !string.IsNullOrEmpty(response.GameUserId))
        {
            await SaveLocalUserData(response.AccountId, response.GameUserId, response.LoginToken);
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
            SessionToken = response.SessionToken,
            GameUserId = response.GameUserId,
            ClientVersion = _clientAppService.Version,
            ClientPlatformName = _clientAppService.GetPlatformName(),
            ClientGameDataSaveTime = _gameData.Get<VersionSettings>(null).SaveTime,
            GameName = _gs.GameMode.ToString(),
        };

        _clientWebService.SendWebRequest(request, token);

    }
}


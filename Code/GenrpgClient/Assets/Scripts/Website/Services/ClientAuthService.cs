using System;
using System.Collections.Generic;
using System.Threading;
using Assets.Scripts.GameSettings.Services;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using UnityEngine;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Client.Core;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.UI.Constants;
using Assets.Scripts.Awaitables;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Core.Interfaces;
using Genrpg.Shared.Accounts.WebApi.Login;
using Genrpg.Shared.Accounts.WebApi.Signup;
using Genrpg.Shared.Versions.Settings;
using System.Linq;
using Newtonsoft.Json.Linq;


public interface IClientAuthService : IInitializable, IClientResetCleanup
{
    void StartAuth(CancellationToken token);
    void Logout();
    void ExitMap();
    Awaitable SendAccountLogin(AccountLoginRequest request, CancellationToken token);
    Awaitable SendSignupRequest(AccountSignupRequest request, CancellationToken token);
    Awaitable SaveLocalUserData(string email, string loginToken);
    Awaitable StartNoUser(CancellationToken token);
    Awaitable OnAccountLogin(AccountLoginResponse response, CancellationToken token);
}

public class ClientAuthService : IClientAuthService
{
    private const string LocalUserFilename = "LocalUser";

    private IClientWebService _clientWebService;
    private IRealtimeNetworkService _realtimeNetworkService;
    private IScreenService _screenService;
    private IMapTerrainManager _mapManager;
    private IClientMapObjectManager _objectManager;
    private IZoneGenService _zoneGenService;
    private IClientGameDataService _gameDataService;
    private IRepositoryService _repoService;
    private ILogService _logService;
    protected IGameData _gameData;
    protected IPlayerManager _playerManager;
    protected IMapProvider _mapProvider;
    protected IClientGameState _gs;
    private IClientConfigContainer _config;
    private IClientCryptoService _clientCryptoService;
    private IClientAppService _clientAppService;
    protected IAwaitableService _awaitableService;
    private ITextSerializer _serializer;

    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public void StartAuth(CancellationToken token)
    {
        LocalUserData localData = _repoService.Load<LocalUserData>(LocalUserFilename).Result;

        string userid = "";
        string email = "";
        string password = "";
        
        if (localData != null)
        {
            try
            {
                userid = localData.UserId;
                password = _clientCryptoService.DecryptString(localData.LoginToken);
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "StartLogin");
            }
        }
        if ((!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(userid)) && !string.IsNullOrEmpty(password))
        {
            AccountLoginRequest LoginRequest = new AccountLoginRequest()
            {
                UserId = userid,
                Email = email,
                Password = password,
                DeviceId = _clientCryptoService.GetDeviceId(),
            };

            _awaitableService.ForgetAwaitable(SendAccountLogin(LoginRequest, token));
            _screenService.Open(ScreenNames.Loading, true);
            return;
        }

        // Otherwise we either had no local login or we had no valid online login, and in this case
        // show the login screen.      
        _screenService.Open(ScreenNames.Login, true);
        _screenService.Close(ScreenNames.Loading);

    }

    public void Logout()
    {
        _logService.Info("Logging out");
        ExitMMOMap();
        _gs.user = null;
        _screenService.CloseAll();
        _screenService.Close(ScreenNames.HUD);
        _screenService.Open(ScreenNames.Login);
    }

    public void ExitMap()
    {
        _logService.Info("Exiting Map");
        ExitMMOMap();
        _screenService.CloseAll();
        _screenService.Close(ScreenNames.HUD);
        _screenService.Open(ScreenNames.CharacterSelect);
    }

    private void ExitMMOMap()
    {
        _zoneGenService.CancelMapToken();
        _playerManager.SetUnit(null);
        _realtimeNetworkService.CloseClient();
        _mapManager.Clear();
        _objectManager.Reset();
        UnityZoneGenService.LoadedMapId = null;
        _mapProvider.SetMap(null);
        _mapProvider.SetSpawns(null);
        _gs.ch = null;

    }
    public async Awaitable SaveLocalUserData(string userId, string loginToken)
    {
        LocalUserData localUserData = new LocalUserData()
        {
            Id = LocalUserFilename,
            UserId = userId,
            LoginToken = _clientCryptoService.EncryptString(loginToken),
        };

        await _repoService.Save(localUserData);
    }

    public async Awaitable SendAccountLogin(AccountLoginRequest request, CancellationToken token)
    {
        request.AccountProductId = _config.Config.AccountProductId;

        AccountLoginResponse result = await _clientWebService.SendAccountAuthWebRequestAsync<AccountLoginResponse>(request, token);

        if (result == null)
        {
            _logService.Info("Got null result on send of " + request.GetType().Name);
        }
    }

    public async Awaitable StartNoUser(CancellationToken token)
    {
        GameAuthResponse result = new GameAuthResponse() { User = new User() { Id = "Local" } };

        WebServerResponseSet resultSet = new WebServerResponseSet() { Responses = new List<IWebResponse>() { result } };

        string txt = _serializer.SerializeToString(resultSet);
        _clientWebService.HandleResponses(txt, null, token);
        await Task.CompletedTask;
    }

    public async Awaitable SendSignupRequest(AccountSignupRequest request, CancellationToken token)
    {
        request.AccountProductId = _config.Config.AccountProductId;
        _clientWebService.SendAccountAuthWebRequest(request, token);
        await Task.CompletedTask;
    }

    public async Task OnClientResetCleanup(CancellationToken token)
    {
        ExitMMOMap();
        await Task.CompletedTask;
    }

    public async Awaitable OnAccountLogin(AccountLoginResponse response, CancellationToken token)
    {
        if (!string.IsNullOrEmpty(response.AccountId) && !string.IsNullOrEmpty(response.LoginToken))
        {
            await SaveLocalUserData(response.AccountId, response.LoginToken);
        }

        GameAuthRequest request = new GameAuthRequest()
        {
            AccountId = response.AccountId,
            ProductAccountId = response.ProductAccountId,
            SessionId = response.SessionId,
            ClientVersion = _clientAppService.Version,
            ClientGameDataSaveTime = _gameData.Get<VersionSettings>(null).SaveTime,
        };

        _clientWebService.SendGameAuthWebRequest(request, token);

    }
}
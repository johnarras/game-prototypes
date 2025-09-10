using Assets.Scripts.Assets;
using Assets.Scripts.Awaitables;
using Assets.Scripts.Core;
using Assets.Scripts.GameSettings.Services;
using Genrpg.Shared.Accounts.WebApi.NewVersions;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.Updates;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Core.Interfaces;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Utils;
using System.Reflection;
using System.Threading;
using UnityEngine;


public class InitClient : BaseBehaviour, IInitClient
{

    [SerializeField]
    private ClientConfig _clientConfig;

    [SerializeField]
    private SplashOverlay _splashOverlay = null;

    [SerializeField]
    private GameObject _globalRoot;

    public bool IsSelfContainedClient()
    {
        return _clientConfig.SelfContainedClient;
    }

    private IClientAuthService _loginService;
    private ICrawlerService _crawlerService;
    private IClientConfigContainer _config;
    private IClientAppService _clientAppService;
    private ICursorService _cursorService;
    private ILocalLoadService _localLoadService;
    private IAwaitableService _awaitableService;
    private ITextSerializer _serializer;
    private IClientGameDataService _gameDataService;

#if UNITY_EDITOR
    public string CurrMapId;
    public static InitClient EditorInstance { get; set; }
    public int BlockCount;
    public float ZoneSize;
    public int ForceZoneTypeId;
    public int MapGenSeed;
    public float PlayerSpeedMult;
    public long AccountSuffixId;
    public bool TestLocalBundles;
#endif

    public EGameModes GameMode => _clientConfig.GameMode;

    private CancellationTokenSource _gameTokenSource = new CancellationTokenSource();

    private void Awake()
    {
        ReflectionUtils.AddAllowedAssembly(Assembly.GetExecutingAssembly());
    }

    public object GetRootObject()
    {
        return _globalRoot;
    }

    public void FullResetGame()
    {
        _awaitableService.ForgetAwaitable(FullResetGameAsync());
    }

    public void HideSplashScreen()
    {
        _splashOverlay.gameObject.SetActive(false);
    }

    public void ShowSplashScreen(string message = null, bool showResetButton = false)
    {
        _screenService?.CloseAll();
        _splashOverlay.gameObject.SetActive(true);
        _splashOverlay.Show(this, message, showResetButton);
    }

    private async Awaitable CleanupGameAsync()
    {
        ShowSplashScreen();
        foreach (IInjectable inj in _gs.loc.GetVals())
        {
            if (inj is IClientResetCleanup cleanup)
            {
                await cleanup.OnClientResetCleanup(GetGameToken());
            }
        }
        _clientEntityService.DestroyAllChildren(_globalRoot);

        _gameTokenSource?.Cancel();
        _gameTokenSource?.Dispose();
        _gameTokenSource = new CancellationTokenSource();
        ClearToken();
        _globalUpdater = null;
    }

    private async Awaitable FullResetGameAsync()
    {
        await CleanupGameAsync();
        await InitGameAsync();
    }

    async void Start()
    {
        await InitGameAsync();
    }

    public async Awaitable<IClientGameState> InitialSetup(bool loadPrefabs)
    {
        _gs = new ClientGameState(_clientConfig, this);
        _gs.GameMode = GameMode;
        ClientSetupService clientInitializer = new ClientSetupService();
        _gs.loc.Resolve(this);
        await clientInitializer.SetupGame(_gs.loc, GetToken());
        _gs.loc.Resolve(this);
        if (loadPrefabs)
        {
            InitialPrefabLoader prefabLoader = _localLoadService.LocalLoad<InitialPrefabLoader>("Prefabs/PrefabLoader");
            await prefabLoader.LoadPrefabs(_gs, _clientEntityService, _localLoadService, _globalRoot);
        }
        await clientInitializer.FinalInitialize(_gs.loc, GetGameToken());
        _gs.loc.Resolve(this);
        return _gs;
    }

    async Awaitable InitGameAsync()
    {
#if UNITY_EDITOR
        EditorInstance = this;
#endif
        await InitialSetup(true);

        string envName = _config.Config.Env.ToString();

        _awaitableService.ForgetAwaitable(DelayRemoveSplashScreen(GetGameToken()));

        _logService.Info("GAME MODE: " + GameMode.ToString());
        // Initial app appearance.
        _clientAppService.TargetFrameRate = 30;
        //_clientAppService.SetupScreen(1920, 1080, false, true, 0);
        _dispatcher.AddListener<NewVersionResponse>(OnNewVersion, GetGameToken());

        while (!_assetService.IsInitialized())
        {
            await Awaitable.WaitForSecondsAsync(0.1f);
        }

        _cursorService.SetCursor(CursorNames.Default);

        await _gameDataService.LoadCachedSettings(_gs);

        await _screenService.OpenAsync(ScreenNames.Loading, null, GetToken());

        _screenService.Open(ScreenNames.FloatingText);
        _screenService.Open(ScreenNames.DynamicUI);

        if (!GameModeUtils.IsPureClientMode(_gs.GameMode))
        {
            _loginService.StartAuth(GetGameToken());
        }
        else
        {
            await _loginService.StartNoUser(GetGameToken());
        }
        string txt2 = "ScreenWH: " + _clientAppService.ScreenWidth + "x" + _clientAppService.ScreenHeight + " -- " + Game.Prefix + " -- " + _config.Config.Env + " -- " + _clientAppService.Platform;
        _logService.Info(txt2);
    }


    void OnApplicationQuit()
    {
        _gameTokenSource.Cancel();
        _gameTokenSource.Dispose();
        _gameTokenSource = null;
        _networkService?.CloseClient();
        _crawlerService?.SaveGame();
    }

    public CancellationToken GetGameToken()
    {
        return _gameTokenSource.Token;
    }

    private async Awaitable DelayRemoveSplashScreen(CancellationToken token)
    {
        while (_screenService == null || _screenService.GetAllScreens().Count < 1)
        {
            await Awaitable.NextFrameAsync(token);
        }

        HideSplashScreen();
    }

    private void OnNewVersion(NewVersionResponse newVersion)
    {
        ShowSplashScreen("New Version Available", false);
    }

    private IGlobalUpdater _globalUpdater;
    public void SetGlobalUpdater(IGlobalUpdater updater)
    {
        _globalUpdater = updater;
    }

    private void Update()
    {
        _globalUpdater?.OnUpdate();
    }

    private void LateUpdate()
    {
        _globalUpdater?.OnLateUpdate();
    }
}
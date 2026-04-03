using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Purchasing.Services;
using Assets.Scripts.Resets.ClientEvents;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.GameAuth.WebApi.NewVersions;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D;

public interface IInitClient : IInjectable
{
    CoreClientData GetCoreClientData();
    CancellationToken GetGameToken();
}

public class InitClient : BaseBehaviour, IInitClient
{

    [SerializeField]
    private ClientConfig _clientConfig = null;

    [SerializeField]
    private SplashOverlay _splashOverlay = null;


    [SerializeField]
    private CoreClientData _coreClientData = null;

    private IClientAuthService _loginService = null;
    private IClientConfigContainer _config = null;
    private IClientAppService _clientAppService = null;
    private ICursorService _cursorService = null;
    private IAwaitableService _awaitableService = null;
    private IClientPurchasingService _purchasingService = null;
    protected IScreenService _screenService = null;

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
    public bool RunSteamInEditor;
#endif

    public EGameModes GameMode => _clientConfig.GameMode;

    private CancellationTokenSource _gameTokenSource = new CancellationTokenSource();

    public CancellationToken GetGameToken()
    {
        return _gameTokenSource?.Token ?? CancellationToken.None;
    }

    private void Awake()
    {
        SpriteAtlasManager.atlasRequested += DummyRequestAtlas;
        SpriteAtlasManager.atlasRegistered += DummyRegisterAtlas;
    }

    public CoreClientData GetCoreClientData()
    {
        return _coreClientData;
    }


    public void OnFullResetGame(FullResetGame full)
    {
        FullResetGameInternal();
    }

    private void FullResetGameInternal()
    {
        _awaitableService.ForgetAwaitable(FullResetGameAsync());
    }

    public void HideSplashScreen()
    {
        _splashOverlay.gameObject.SetActive(false);
    }

    private void ShowSplashScreenInternal(string message = null, bool showResetButton = false)
    {
        _dispatcher.Dispatch(new CloseAllScreens());
        _splashOverlay.gameObject.SetActive(true);
        _splashOverlay.Show(FullResetGameInternal, message, showResetButton);
    }

    private async Awaitable CleanupGameAsync()
    {
        ShowSplashScreenInternal();
        foreach (IClientResetCleanup cleanup in _gs.loc.GetVals<IClientResetCleanup>())
        {
            await cleanup.OnReset(GetGameToken());
        }
        _gameTokenSource?.Cancel();
        _gameTokenSource?.Dispose();
        _gameTokenSource = new CancellationTokenSource();
        ClearToken();
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

    public async Awaitable<IClientGameState> InitialSetup()
    {
        _gs = new ClientGameState(_clientConfig, this);
        _gs.GameMode = GameMode;
        ClientSetupService clientInitializer = new ClientSetupService();
        _gs.loc.Resolve(this);
        await clientInitializer.SetupGame(_gs, new List<object> { this }, GetToken());
        _clientAppService.ShowCurrentScreenState();
        return _gs;
    }

    async Awaitable InitGameAsync()
    {
#if UNITY_EDITOR
        EditorInstance = this;
#endif
        await InitialSetup();


        string envName = _config.Config.Env.ToString();

        _awaitableService.ForgetAwaitable(DelayRemoveSplashScreen(GetGameToken()));

        _logService.Info("GAME MODE: " + GameMode.ToString());
        // Initial app appearance.
        _clientAppService.TargetFrameRate = 60;
        _dispatcher.AddListener<NewVersionResponse>(OnNewVersion, GetGameToken());
        _dispatcher.AddListener<FullResetGame>(OnFullResetGame, GetGameToken());
        _dispatcher.AddListener<ShowSplashScreen>(OnShowSplashScreen, GetGameToken());

        while (!_assetService.IsInitialized() ||
            !_screenService.IsInitialized())
        {
            await Awaitable.WaitForSecondsAsync(0.1f, GetGameToken());
        }

        _cursorService.SetCursor(CursorNames.Default);

        await _screenService.OpenAsync(ScreenNames.Loading, null, GetGameToken());

        await _purchasingService.InitializeStores(GetToken());

        _dispatcher.Dispatch(new OpenScreen(ScreenNames.FloatingText));
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.DynamicUI));

        if (!GameModeUtils.IsPureClientMode(_gs.GameMode))
        {
            _loginService.StartAuth(GetGameToken());
        }
        else
        {
            await _loginService.StartNoUser(GetGameToken());
        }
        string txt2 = "ScreenWH: " + _clientAppService.ScreenWidth + "x" + _clientAppService.ScreenHeight + " -- " + Game.Prefix + " -- " + _config.Config.Env + " -- " + _clientAppService.RuntimePlatform;
        _logService.Info(txt2);
    }


    void OnApplicationQuit()
    {
        ShowSplashScreenInternal();
        _gameTokenSource.Cancel();
        _gameTokenSource.Dispose();
        _gameTokenSource = null;
        if (_gs != null && _gs.loc != null)
        {
            foreach (IClientQuitCleanup cleanup in _gs.loc.GetVals<IClientQuitCleanup>())
            {
                cleanup.OnQuit();
            }
        }
    }

    private async Awaitable DelayRemoveSplashScreen(CancellationToken token)
    {
        while (_screenService == null || _screenService.GetAllScreens().Count < 1)
        {
            await Awaitable.NextFrameAsync(token);
        }

        HideSplashScreen();
    }

    private void OnShowSplashScreen(ShowSplashScreen showSplashScreen)
    {
        ShowSplashScreenInternal(showSplashScreen.Message, showSplashScreen.ShowResetButton);
    }

    private void OnNewVersion(NewVersionResponse newVersion)
    {
        ShowSplashScreenInternal("New Version Available", false);
    }

    protected override void OnDestroy()
    {
        SpriteAtlasManager.atlasRequested -= DummyRequestAtlas;
        SpriteAtlasManager.atlasRegistered -= DummyRegisterAtlas;
    }

    private void DummyRequestAtlas(string tag, System.Action<SpriteAtlas> callback)
    {

    }

    private void DummyRegisterAtlas(SpriteAtlas callback)
    {

    }

}


using Assets.Scripts.Options.Services;
using OxDb.SharedCore.Client.Contants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.DataStores.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.Profiling.Memory;
using UnityEngine; // Needed


public interface IClientAppService : IInitializable, IExplicitInject
{
    void Quit();
    int TargetFrameRate { get; set; }
    string DataPath { get; }
    string PersistentDataPath { get; }
    bool IsPlaying { get; }
    bool IsEditor { get; }
    string Version { get; }
    string RuntimePlatform { get; }
    string DeviceUniqueIdentifier { get; }
    string StreamingAssetsPath { get; }
    void OpenExternalURL(string url);
    string GetPlatformName();
    string GetRuntimePrefix();
    void SetupScreen(int width, int height, bool isFullScreen, bool isLandscape, int vsyncCount);
    void SetOrientation(ScreenOrientation orientation);
    int ScreenWidth { get; }
    int ScreenHeight { get; }
    Awaitable TakeMemorySnapshot();
    bool IsFullScreen();
    void SetFullScreen(bool isFullScreen);
    void ShowCurrentScreenState();
    float GetDeltaTime();
    float TotalElapsedTime();
}



public class ClientAppService : IClientAppService
{

    protected ILogService _logService = null;
    protected IClientOptionsService _optionsService = null;

    public ClientAppService(ILogService logService)
    {
        _logService = logService;
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public int TargetFrameRate
    {
        get { return Application.targetFrameRate; }
        set { Application.targetFrameRate = value; }
    }

    public int ScreenWidth => Screen.width;
    public int ScreenHeight => Screen.height;
    public string DataPath => Application.dataPath;

    public string PersistentDataPath => Application.persistentDataPath;

    public string StreamingAssetsPath => Application.streamingAssetsPath;

    public bool IsPlaying => Application.isPlaying;

    public bool IsEditor => Application.isEditor;


    private DateTime _startTime = DateTime.UtcNow;


    public async Task Initialize(CancellationToken token)
    {

        await Task.CompletedTask;
    }

    public void OpenExternalURL(string url) { Application.OpenURL(url); }

    public string GetPlatformName()
    {

        string prefix = ClientPlatformNames.Win;
#if UNITY_STANDALONE_OSX
        prefix = ClientPlatformNames.OSX;
#endif
#if UNITY_STANDALONE_LINUX
        prefix = ClientPlatformNames.Linux;
#endif
#if UNITY_ANDROID
        prefix = ClientPlatformNames.Android;
#endif
#if UNITY_IOS
        prefix = ClientPlatformNames.IOS;
#endif
        return prefix;
    }

    private string _runtimePrefix = null;
    public string GetRuntimePrefix()
    {
        if (!string.IsNullOrEmpty(_runtimePrefix))
        {
            return _runtimePrefix;
        }

        _runtimePrefix = BlobUtils.GetBlobSubfolder(Version, GetPlatformName());
        return _runtimePrefix;
    }

    public string Version => Application.version;

    public string RuntimePlatform => Application.platform.ToString();

    public string DeviceUniqueIdentifier => SystemInfo.deviceUniqueIdentifier;

    private FullScreenMode _fullScreenMode = FullScreenMode.Windowed;

    public void SetupScreen(int width, int height, bool isFullScreen, bool isLandscape, int vsyncCount)
    {

        if (!IsPlaying)
        {
            return;
        }
        _fullScreenMode = isFullScreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;

        Screen.SetResolution(width, height, _fullScreenMode);
        Screen.orientation = isLandscape ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
        QualitySettings.vSyncCount = vsyncCount;
    }

    private void DisableAutoRotation()
    {
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToLandscapeLeft = false;
    }

    public void SetOrientation(ScreenOrientation orientation)
    {
        Screen.orientation = orientation;
        DisableAutoRotation();
    }

    public async Awaitable TakeMemorySnapshot()
    {

        MemoryProfiler.TakeSnapshot(Path.Combine(PersistentDataPath, "Snapshots/MemSnapshot.snap"), OnTakeSnapshot);


        await Task.CompletedTask;
    }

    private void OnTakeSnapshot(string txt, bool val)
    {
    }

    public bool IsFullScreen()
    {
        return Screen.fullScreen;
    }

    public void ShowCurrentScreenState()
    {
        LocalClientOptions options = _optionsService.GetOptions();
        SetFullScreen(options.HasFlag(ClientFlags.IsFullScreen));
    }

    public void SetFullScreen(bool isFullScreen)
    {
        LocalClientOptions options = _optionsService.GetOptions();
        if (isFullScreen)
        {
            if (Screen.width < Screen.currentResolution.width)
            {
                options.ScreenWidth = Screen.width;
            }
            if (Screen.height < Screen.currentResolution.height)
            {
                options.ScreenHeight = Screen.height;
            }
            options.AddFlags(ClientFlags.IsFullScreen);
            SetupScreen(Screen.currentResolution.width, Screen.currentResolution.height, isFullScreen, Screen.orientation == ScreenOrientation.LandscapeLeft, QualitySettings.vSyncCount);
        }
        else
        {
            options.RemoveFlags(ClientFlags.IsFullScreen);
            SetupScreen(options.ScreenWidth, options.ScreenHeight, false, Screen.orientation == ScreenOrientation.LandscapeLeft, QualitySettings.vSyncCount);
        }
        _optionsService.SaveOptions();
    }

    public float GetDeltaTime()
    {
        return Time.deltaTime;
    }

    public float TotalElapsedTime()
    {
        return (float)(DateTime.UtcNow - _startTime).TotalSeconds;
    }
}







using Genrpg.Shared.Client.Contants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
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
    int ScreenWidth { get; }
    int ScreenHeight { get; }
    Awaitable TakeMemorySnapshot();
    bool IsFullScreen();
    void SetFullScreen(bool isFullScreen);
}



public class ClientAppService : IClientAppService
{

    protected ILogService _logService = null;

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

        string prefix = GetPlatformName();
        _runtimePrefix = Version + "/" + prefix + "/";
        return _runtimePrefix;
    }

    public string Version => Application.version;

    public string RuntimePlatform => Application.platform.ToString();

    public string DeviceUniqueIdentifier => SystemInfo.deviceUniqueIdentifier;

    private FullScreenMode _fullScreenMode = FullScreenMode.Windowed;

    public void SetupScreen(int width, int height, bool isFullScreen, bool isLandscape, int vsyncCount)
    {

        _fullScreenMode = isFullScreen ? FullScreenMode.MaximizedWindow : FullScreenMode.Windowed;

        Screen.SetResolution(width, height, _fullScreenMode);
        Screen.orientation = isLandscape ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        QualitySettings.vSyncCount = vsyncCount;
    }

    public async Awaitable TakeMemorySnapshot()
    {
        Debug.Log("Snapshot1");

        MemoryProfiler.TakeTempSnapshot(OnTakeSnapshot);

        Debug.Log("Snapshot2");

        await Task.CompletedTask;
    }

    private void OnTakeSnapshot(string txt, bool val)
    {
        Debug.Log("Snapshot: " + txt);
    }

    public bool IsFullScreen()
    {
        return Screen.fullScreen;
    }

    public void SetFullScreen(bool isFullScreen)
    {
        SetupScreen(Screen.width, Screen.height, isFullScreen, Screen.orientation == ScreenOrientation.LandscapeLeft, QualitySettings.vSyncCount);
    }
}




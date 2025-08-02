
using Genrpg.Shared.Interfaces;
using Scripts.Assets.Assets.Constants;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine; // Needed


public interface IClientAppService : IInitializable
{
    void Quit();
    int TargetFrameRate { get; set; }
    string DataPath { get; }
    string PersistentDataPath { get; }
    bool IsPlaying { get; }
    bool IsEditor { get; }
    string Version { get; }
    string Platform { get; }
    string DeviceUniqueIdentifier { get; }
    string StreamingAssetsPath { get; }
    void OpenExternalURL(string url);
    string GetPlatformPrefix();
    string GetRuntimePrefix();
    void SetupScreen(int width, int height, bool isFullScreen, bool isLandscape, int vsyncCount);
    int ScreenWidth { get; }
    int ScreenHeight { get; }
}



public class ClientAppService : IClientAppService
{

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

    public string GetPlatformPrefix()
    {

        string prefix = PlatformAssetPrefixes.Win;
#if UNITY_STANDALONE_OSX
        prefix = PlatformAssetPrefixes.OSX;
#endif
#if UNITY_STANDALONE_LINUX
        prefix = PlatformAssetPrefixes.Linux;
#endif
#if UNITY_ANDROID
        prefix = PlatformAssetPrefixes.Android;
#endif
#if UNITY_IOS
        prefix = PlatformAssetPrefixes.IOS;
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

        string prefix = GetPlatformPrefix();
        _runtimePrefix = Version + "/" + prefix + "/";
        return _runtimePrefix;
    }

    public string Version => Application.version;

    public string Platform => Application.platform.ToString();

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

    public int ScreenWidth => Screen.width;
    public int ScreenHeight => Screen.height;
}




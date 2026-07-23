using OxDb.Client.Config;
using OxDb.Client.Core.Interfaces;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.LevelPlay;

namespace OxDb.Client.Ads.Services
{

    public interface IClientAdsService : IInitializable, IClientResetCleanup
    {

    }

    public class ClientAdsService : IClientAdsService
    {
        private IClientConfigContainer _configContainer = null;
        private ILogService _logService = null;
        private IClientAppService _appService = null;

        private string _gameKey = "";
        private string _userId = "";

        private bool _isProd = false;

        public async Task Initialize(CancellationToken token)
        {

            _isProd = EnvNames.IsProdEnv(_configContainer.Config.Env);

#if UNITY_IOS
            _adsGameKey = _configContainer.Config.IOSAdsGameKey;
#else
            _gameKey = _configContainer.Config.AndroidAdsGameKey;
#endif


#if UNITY_EDITOR
            XmlDict dict = XmlUtils.ExtractAppConfigData(ConfigConstants.MainAppConfigPath);

            string configGameKey = "";

#if UNITY_IOS
            configGameKey =  dict.GetVal(AppConfigKeys.UnityIOSAdsGameKey);
#else
            configGameKey = dict.GetVal(AppConfigKeys.UnityAndroidAdsGameKey);
#endif

            if (string.IsNullOrEmpty(_gameKey) && !string.IsNullOrEmpty(configGameKey))
            {
                _gameKey = configGameKey;
            }
#endif

            LevelPlay.OnInitSuccess += OnLevelPlayInitSuccess;
            LevelPlay.OnInitFailed += OnLevelPlayInitFailed;

            InitializeLevelPlay();
            await Task.CompletedTask;
        }

        public async Task OnReset(CancellationToken token)
        {
            LevelPlay.OnInitSuccess -= OnLevelPlayInitSuccess;
            LevelPlay.OnInitFailed -= OnLevelPlayInitFailed;
            await Task.CompletedTask;
        }

        private void InitializeLevelPlay()
        {
            if (string.IsNullOrEmpty(_gameKey))
            {
                _logService.Error("LevelPlay App Key (_gameKey) is missing or empty!");
                return;
            }

            // Fallback to device unique identifier if no user ID is specified
            if (string.IsNullOrEmpty(_userId))
            {
                _userId = _appService.DeviceUniqueIdentifier;
            }

            _logService.Info("Initializing LevelPlay SDK...");
            LevelPlay.Init(_gameKey, _userId);
        }

        private void OnLevelPlayInitSuccess(LevelPlayConfiguration configuration)
        {
            _logService.Info("LevelPlay SDK Initialized Successfully!");

            // Your SDK is ready. You can now safely instantiate and load your ads here.
            // Example: LoadInterstitial();
        }

        private void OnLevelPlayInitFailed(LevelPlayInitError error)
        {
            _logService.Info($"LevelPlay SDK Initialization Failed: {error.ErrorMessage} (Code: {error.ErrorCode})");
        }
    }
}
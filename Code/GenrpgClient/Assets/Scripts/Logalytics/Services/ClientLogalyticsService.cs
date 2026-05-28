using Assets.Scripts.Logalytics.ClientEvents;
using OxDb.SharedCore.Core.Entities;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Logalytics.Services
{
    public interface IClientLogalyticsService : IInitializable
    {
        Dictionary<string, string> GetDefaultLogalyticsDimensions();
    }

    public class ClientLogalyticsService : IClientLogalyticsService
    {
        private IClientGameState _gs = null;
        private IClientAppService _appService = null;
        private IClientWebService _webService = null;
        private IClientConfigContainer _configContainer = null;
        private IDispatcher _dispatcher = null;

        public async Task Initialize(CancellationToken token)
        {

            _dispatcher.AddListener<UpdateDefaultLogalyticsPayload>(OnUpdateDefaultLogalyticsPayload, token);
            await Task.CompletedTask;
        }

        private void OnUpdateDefaultLogalyticsPayload(UpdateDefaultLogalyticsPayload updateDefault)
        {
            UpdateLogalyticsDimensions();
        }

        private void UpdateLogalyticsDimensions()
        {
            _cachedData = new Dictionary<string, string>();

            _cachedData[LogalyticsKeys.GameUserId] = _gs.GameUserId;
            _cachedData[LogalyticsKeys.ClientVersion] = _appService.Version;
            _cachedData[LogalyticsKeys.ClientPlatform] = _appService.GetPlatformName();

            _cachedData[LogalyticsKeys.RequestId] = _webService.GetUserRequestId();
            _cachedData[LogalyticsKeys.ClientEnv] = _configContainer.Config.Env;
            _cachedData[LogalyticsKeys.ProductName] = _gs.GameMode.ToString();
            _cachedData[LogalyticsKeys.GameComponent] = GameComponentNames.Client;

            if (_gs.SessionState != null)
            {
                _cachedData[LogalyticsKeys.ClientSessionId] = _gs.ClientSessionId;
                _cachedData[LogalyticsKeys.ServerVersion] = _gs.SessionState.ServerVersion;
                _cachedData[LogalyticsKeys.ServerEnv] = _gs.SessionState.ServerEnv;
            }

            List<string> keys = _cachedData.Keys.ToList();

            foreach (string key in keys)
            {
                if (_cachedData[key] == null)
                {
                    _cachedData.Remove(key);
                }
            }
        }

        private Dictionary<string, string> _cachedData = null;
        public Dictionary<string, string> GetDefaultLogalyticsDimensions()
        {

            if (_cachedData == null)
            {
                UpdateLogalyticsDimensions();
            }
            return _cachedData;
        }
    }
}

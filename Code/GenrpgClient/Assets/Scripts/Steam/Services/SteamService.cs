
#if !DISABLESTEAMWORKS
using OxDb.Client.GameObjects;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using Steamworks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Steam.Services
{

    public interface ISteamService : IInitializable
    {
        void OnUpdate();

        void OnQuit();

        void OnMessage(StringBuilder sb);
    }

    public class SteamService : ISteamService
    {
        private ILogService _logService = null;
        private ISingletonContainer _singletonContainer = null;
        private IClientEntityService _clientEntityService = null;

        private static SteamManager _steamManager { get; set; } = null;
        public async Task Initialize(CancellationToken token)
        {
#if UNITY_EDITOR
            if (InitClient.EditorInstance == null || !InitClient.EditorInstance.RunSteamInEditor)
            {
                return;
            }
#endif

            if (_steamManager == null)
            {
                GameObject steamObject = _singletonContainer.GetSingleton("SteamManager");

                _steamManager = _clientEntityService.GetOrAddComponent<SteamManager>(steamObject);
            }

            await Task.CompletedTask;
        }

        public void OnMessage(StringBuilder sb)
        {

            _logService.Info("Steam Message: " + sb.ToString());
        }

        public void OnUpdate()
        {
            // Run Steam client callbacks
            SteamAPI.RunCallbacks();
        }

        public void OnQuit()
        {
            _logService.Info("Steam Shut Down");
        }
    }
}

#endif
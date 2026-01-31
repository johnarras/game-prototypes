using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Minigames.Controllers;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Client.Tokens;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Minigames.Games.WebApi;
using Genrpg.Shared.Trader.MinigameTypes.Settings;
using Genrpg.Shared.UI.Constants;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Minigames.Services
{
    public interface IClientMinigameService : IInitializable, IGameTokenService
    {
        void ShowMinigame(long minigameTypeId);
        void ShowLobby(long offset = 0);

        void ClickWin(long minigameTypeId);
        void ClickLose(long minigameTypeId);
        GameObject MinigameAnchor { get; }
    }

    public class ClientMinigameService : IClientMinigameService
    {
        private IAssetService _assetService = null;
        private ISingletonContainer _singletonContainer = null;

        private IDispatcher _dispatcher = null;
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IClientEntityService _clientEntityService = null;
        private IAwaitableService _awaitableService = null;
        private IScreenService _screenService = null;
        private ICameraController _cameraController = null;
        private IClientWebService _webService = null;

        private GameObject _minigameAnchor;

        public GameObject MinigameAnchor => _minigameAnchor;

        private CancellationToken _token;
        public void SetGameToken(CancellationToken token)
        {
            _token = token;
        }


        public async Task Initialize(CancellationToken token)
        {
            _minigameAnchor = _singletonContainer.GetSingleton("MinigameAnchor");

            Camera camera = _cameraController.GetMainCamera();
            camera.transform.position = new Vector3(0, 0, 10);
            camera.transform.LookAt(Vector3.zero);
            await Task.CompletedTask;
        }

        public void ShowMinigame(long minigameTypeId)
        {
            _awaitableService.ForgetAwaitable(ShowMinigameAsync(minigameTypeId));
        }

        private async Awaitable ShowMinigameAsync(long minigameTypeId)
        { 
            MinigameType mtype = _gameData.Get<MinigameTypeSettings>(_gs.ch).Get(minigameTypeId);

            if (mtype == null)
            {
                _dispatcher.Dispatch(new ShowFloatingText("Missing that minigame!", EFloatingTextArt.Error));
                return;
            }

            _dispatcher.Dispatch(new CloseAllScreens());
            await _screenService.OpenAsync(ScreenNames.Loading, null, _token);
            _clientEntityService.DestroyAllChildren(_minigameAnchor);
            _assetService.LoadAssetInto(_minigameAnchor, AssetCategoryNames.Minigames, mtype.Art, OnLoadMinigame, _token, mtype, mtype.ArtSubdirectory);
                
        }

        public void ShowLobby(long offset = 0)
        {
            _awaitableService.ForgetAwaitable(ShowLobbyAsync(offset, _token));
        }

        private async Awaitable ShowLobbyAsync(long offset = 0, CancellationToken token = default)
        {

            _dispatcher.Dispatch(new OpenScreen(ScreenNames.Loading));
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.MinigameHUD));
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.MinigameLobby));

            while (_screenService.GetScreen(ScreenNames.MinigameHUD) == null ||
                _screenService.GetScreen(ScreenNames.MinigameLobby) == null)
            {
                await Awaitable.NextFrameAsync(token);
            }
            _clientEntityService.DestroyAllChildren(MinigameAnchor);

            _dispatcher.Dispatch(new CloseScreen(ScreenNames.Loading));
        }

        private void OnLoadMinigame(GameObject go, MinigameType mtype, CancellationToken token)
        {

            BaseMinigameController controller = go.GetComponent<BaseMinigameController>();

            if (controller == null)
            {
                _clientEntityService.Destroy(go);

                ShowLobby();
                return;
            }

            controller.SetData(mtype);

            _dispatcher.Dispatch(new CloseAllScreens());
        }

        public void ClickWin(long minigameTypeId)
        {
            MinigameType mtype = _gameData.Get<MinigameTypeSettings>(_gs.ch).Get(minigameTypeId);
            _dispatcher.Dispatch(new ShowFloatingText("Won " + mtype.Name));
            ShowLobby(minigameTypeId);
            _webService.SendClientUserWebRequest(new EndMinigameRequest() { MinigameTypeId = minigameTypeId, WonGame = true }, _token);
        }

        public void ClickLose(long minigameTypeId)
        {
            MinigameType mtype = _gameData.Get<MinigameTypeSettings>(_gs.ch).Get(minigameTypeId);
            _dispatcher.Dispatch(new ShowFloatingText("Lost " + mtype.Name));
            ShowLobby(minigameTypeId);
            _webService.SendClientUserWebRequest(new EndMinigameRequest() { MinigameTypeId = minigameTypeId, WonGame = false }, _token);
        }
    }
}

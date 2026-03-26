using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.GameSettings.Entities;
using Assets.Scripts.Lockstep.Config.Core;
using Assets.Scripts.Lockstep.Game.Services;
using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.Minigames.Services;
using Assets.Scripts.Purchasing.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.GameAuth.WebApi.Auth;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.MapServer.WebApi.UploadMap;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.UI.Constants;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class GameAuthResponseHandler : BaseClientWebResponseHandler<GameAuthResponse>
    {
        private IScreenService _screenService = null;
        private IAssetService _assetService = null;
        private IClientWebService _webNetworkService = null;
        private IClientPurchasingService _purchasingService = null;
        private IClientMinigameService _clientMinigameService = null;
        private ILockstepGameService _lockstepService = null;

        protected override async Awaitable InnerProcess(GameAuthResponse response, CancellationToken token)
        {
            _awaitableService.ForgetAwaitable(InnerProcessAsync(response, token));
        }

        private async Awaitable InnerProcessAsync(GameAuthResponse response, CancellationToken token)
        {
            List<long> keepOpenScreens = new List<long>();
            if (_screenService.GetScreen(ScreenNames.Signup) != null)
            {
                keepOpenScreens.Add(ScreenNames.Signup);
            }
            if (_screenService.GetScreen(ScreenNames.Login) != null)
            {
                keepOpenScreens.Add(ScreenNames.Login);
            }

            if (response == null || string.IsNullOrEmpty(response.GameUserId) ||
                string.IsNullOrEmpty(response.SessionToken))
            {
                _dispatcher.Dispatch(new CloseAllScreens(keepOpenScreens));
                if (keepOpenScreens.Count < 1)
                {
                    _dispatcher.Dispatch(new OpenScreen(ScreenNames.Login));
                }
                return;
            }

            keepOpenScreens.Clear();
            _gs.GameUserId = response.GameUserId;
            _gs.SessionState = response;
            _gs.characterStubs = response.CharacterStubs;
            _gs.mapStubs = response.MapStubs;
            _gs.ch = new Character(new CoreCharacter()) { Id = _gs.GameUserId, UserId = _gs.GameUserId, Name = "StubCharacter" };

            foreach (IUnitData unitData in response.UserData)
            {
                unitData.Id = Guid.NewGuid().ToString();
                if (unitData is CoreDataDto dto)
                {
                    _gs.ch.DataOverrides = dto.Parent.DataOverrides;
                }
                _gs.ch.Set(unitData);
            }

            if (response.OfferData != null)
            {
                _gs.ch.Set(response.OfferData);
            }

            List<ITopLevelSettings> loadedSettings = _gameData.AllSettings();
            if (_gameData is ClientGameData clientGameData)
            {
                clientGameData.SetSettingsObject(_gs.ch);
            }

            await Awaitable.NextFrameAsync(cancellationToken: token);
            await Awaitable.NextFrameAsync(cancellationToken: token);

            bool closeAllScreens = true;
            keepOpenScreens = new List<long>();
            if (GameModeUtils.IsPureClientMode(_gs.GameMode))
            {
                if (_gs.GameMode == EGameModes.Crawler)
                {
                    keepOpenScreens.Add(ScreenNames.CrawlerMainMenu);
                    await _screenService.OpenAsync(ScreenNames.CrawlerMainMenu, null, token);
                }
                else if (_gs.GameMode == EGameModes.LockstepTemplate)
                {
                    BaseLockstepConfig lockstepConfig = await _lockstepService.SetupExampleLockstep(432132);

                    _lockstepService.SetupGame(lockstepConfig);
                }
            }
            else if (_gs.GameMode == EGameModes.Trader)
            {
                keepOpenScreens.Add(ScreenNames.TraderHUD);
                await _screenService.OpenAsync(ScreenNames.TraderHUD, null, token);
            }
            else if (_gs.GameMode == EGameModes.Minigames)
            {

                closeAllScreens = false;
                keepOpenScreens.Add(ScreenNames.MinigameHUD);
                _clientMinigameService.ShowLobby();
            }
            else
            {
                try
                {
                    await _screenService.OpenAsync(ScreenNames.Loading, null, token);
                    _dispatcher.Dispatch(new CloseAllScreens(new List<long>() { ScreenNames.Loading }));
                    keepOpenScreens.Add(ScreenNames.CharacterSelect);
                    _dispatcher.Dispatch(new CloseScreen(ScreenNames.HUD));
                    var screen = await _screenService.OpenAsync(ScreenNames.CharacterSelect, null, token);
                    _logService.Info("Screen: " + screen);
                }
                catch (Exception ex)
                {
                    _logService.Exception(ex, "GameAuthLoginHandler");
                }
            }
            await _purchasingService.RetryPurchaseAfterLogin(token);
            if (closeAllScreens)
            {
                _dispatcher.Dispatch(new CloseAllScreens(keepOpenScreens));
            }
            await Task.CompletedTask;
        }

        public async Awaitable RetryUploadMap(CancellationToken token)
        {
            // Set the mapId you want to upload to here.
            string mapId = "1";

            UploadMapRequest comm = new UploadMapRequest();
            comm.Map = await _repoService.Load<Map>("UploadedMap");
            comm.SpawnData = await _repoService.Load<MapSpawnData>("UploadedSpawns");
            comm.Map.Id = mapId;
            comm.SpawnData.Id = mapId;
            comm.WorldDataEnv = _assetService.GetWorldDataEnv();
            _webNetworkService.SendWebRequest(comm, token);
        }
    }
}



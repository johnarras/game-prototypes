using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.GameSettings.Entities;
using Assets.Scripts.Lockstep.Config.Core;
using Assets.Scripts.Lockstep.Game.Services;
using Assets.Scripts.Logalytics.ClientEvents;
using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.Minigames.Services;
using Assets.Scripts.Purchasing.Services;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.Logalytics.Constants;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.GameAuth.WebApi.Auth;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.MapServer.WebApi.UploadMap;
using OxDb.SharedGame.Spawns.WorldData;
using OxDb.SharedGame.Trader.Flags.Constants;
using OxDb.SharedGame.UI.Constants;
using OxDb.SharedGame.Users.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Auth.ResponseHandlers
{
    public class GameAuthResponseHandler : BaseClientWebResponseHandler<GameAuthResponse>
    {
        private IScreenService _screenService = null;
        private IClientWebService _webNetworkService = null;
        private IClientPurchasingService _purchasingService = null;
        private IClientMinigameService _clientMinigameService = null;
        private ILockstepGameService _lockstepService = null;
        private IUserSnapshotService _snapshotService = null;
        private IAnalyticsService _analyticsService = null;
        private IClientAppService _appService = null;

        protected override async ValueTask InnerProcess(GameAuthResponse response, CancellationToken token)
        {

            List<long> keepOpenScreens = new List<long>()
            {
                ScreenNames.Signup,
                ScreenNames.Login,
                ScreenNames.GetMainAuthScreen(),
            };

            if (response == null || string.IsNullOrEmpty(response.GameUserId) ||
                string.IsNullOrEmpty(response.FullToken) ||
                string.IsNullOrEmpty(response.GameSessionId))
            {
                _dispatcher.Dispatch(new CloseAllScreens(keepOpenScreens));
                if (keepOpenScreens.Count < 1)
                {
                    _dispatcher.Dispatch(new OpenScreen(ScreenNames.GetMainAuthScreen()));
                }
                return;
            }

            keepOpenScreens.Clear();
            _gs.GameUserId = response.GameUserId;
            _gs.SessionState = response;
            _gs.characterStubs = response.CharacterStubs;
            _gs.mapStubs = response.MapStubs;
            _gs.ch = new Character(new CoreCharacter()) { Id = _gs.GameUserId, UserId = _gs.GameUserId, Name = "StubCharacter" };
            _gs.ch.ClientVersion = new Version(_appService.Version);
            _gs.ch.ClientPlatform = _appService.RuntimePlatform;

            _dispatcher.Dispatch(new UpdateDefaultLogalyticsPayload());

            if (response.DidCreateAccount)
            {
                _analyticsService.TrackEvent(AnalyticsEventNames.CreateUser);
            }

            foreach (IUnitData unitData in response.UserData)
            {
                unitData.Id = Guid.NewGuid().ToString();
                if (unitData is CoreDataDto dto)
                {
                    _gs.ch.AB = dto.Parent.AB;
                }
                _gs.ch.Set(unitData);
            }

            if (response.OfferData != null)
            {
                _gs.ch.Set(response.OfferData);
            }


            _logService.Info("Login " + await _snapshotService.GetSnapshotString(_gs.ch));
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
                CoreData coreData = await _gs.ch.GetAsync<CoreData>();
                coreData.AddFlag(TraderFlags.CompletedFtue);
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

        public async ValueTask RetryUploadMap(CancellationToken token)
        {
            // Set the mapId you want to upload to here.
            string mapId = "1";

            UploadMapRequest comm = new UploadMapRequest();
            comm.Map = await _repoService.Load<Map>("UploadedMap");
            comm.SpawnData = await _repoService.Load<MapSpawnData>("UploadedSpawns");
            comm.Map.Id = mapId;
            comm.SpawnData.Id = mapId;
            _webNetworkService.SendWebRequest(comm, token);
        }
    }
}



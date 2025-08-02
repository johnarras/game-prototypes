using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using Genrpg.Shared.GameSettings.Interfaces;
using Assets.Scripts.GameSettings.Services;

using System.Threading;
using Genrpg.Shared.Spawns.WorldData;
using UnityEngine;
using Genrpg.Shared.Characters.PlayerData;
using Assets.Scripts.GameSettings.Entities;
using Assets.Scripts.UI.Interfaces;
using Assets.Scripts.Assets;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Accounts.WebApi.Login;
using Genrpg.Shared.MapServer.WebApi.UploadMap;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Assets.Scripts.BoardGame.Controllers;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class GameAuthResponseHandler : BaseClientWebResponseHandler<GameAuthResponse>
    {
        private IScreenService _screenService = null;
        private IAssetService _assetService = null;
        private IClientWebService _webNetworkService = null;
        private IBoardGameController _boardGameController = null;
        protected override void InnerProcess(GameAuthResponse response, CancellationToken token)
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

            if (response == null || response.User == null)
            {
                _screenService.CloseAll(keepOpenScreens);
                if (keepOpenScreens.Count < 1)
                {
                    _screenService.Open(ScreenNames.Login);
                }
                return;
            }

            keepOpenScreens.Clear();
            _gs.user = response.User;
            _gs.characterStubs = response.CharacterStubs;
            _gs.mapStubs = response.MapStubs;
            _gs.ch = new Character(new CoreCharacter()) { Id = _gs.user.Id, UserId = _gs.user.Id, Name = "StubCharacter" };

            foreach (IUnitData unitData in response.UserData)
            {
                _gs.ch.Set(unitData);
            }

            List<ITopLevelSettings> loadedSettings = _gameData.AllSettings();
            if (_gameData is ClientGameData clientGameData)
            {
                clientGameData.SetFilteredObject(_gs.ch);
            }

            await Awaitable.NextFrameAsync(cancellationToken: token);
            await Awaitable.NextFrameAsync(cancellationToken: token);

            keepOpenScreens = new List<long>();
            if (GameModeUtils.IsPureClientMode(_gs.GameMode))
            {
                keepOpenScreens.Add(ScreenNames.CrawlerMainMenu);
                await _screenService.OpenAsync(ScreenNames.CrawlerMainMenu, null, token);
            }
            else if (_gs.GameMode == EGameModes.BoardGame)
            {
                keepOpenScreens.Add(ScreenNames.MobileHUD);
                await _screenService.OpenAsync(ScreenNames.MobileHUD, null, token);
                _boardGameController.LoadCurrentBoard();
            }
            else
            {
                try
                {
                    await _screenService.OpenAsync(ScreenNames.Loading, null, token);
                    _screenService.CloseAll(new List<long>() { ScreenNames.Loading });
                    keepOpenScreens.Add(ScreenNames.CharacterSelect);
                    _screenService.Close(ScreenNames.HUD);
                    var screen = await _screenService.OpenAsync(ScreenNames.CharacterSelect, null, token);
                    _logService.Info("Screen: " + screen);
                }
                catch (Exception ex)
                {
                    _logService.Exception(ex, "GameAuthLoginHandler");
                }
            }
            _screenService.CloseAll(keepOpenScreens);
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
            _webNetworkService.SendClientUserWebRequest(comm, token);
        }
    }
}

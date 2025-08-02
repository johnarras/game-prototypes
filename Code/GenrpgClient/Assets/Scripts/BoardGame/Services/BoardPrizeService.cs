using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.BoardGame.Tiles;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Prizes.Settings;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Client.Assets;
using Genrpg.Shared.Client.Assets.Constants;
using Assets.Scripts.Assets;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Services
{
    public interface IBoardPrizeService : IInjectable
    {
        Awaitable UpdatePrizes(CancellationToken token);
    }

    public class BoardPrizeService : IBoardPrizeService
    {
        private IBoardGameController _controller;
        private IClientGameState _gs;
        private IGameData _gameData;
        private IAssetService _assetService;
        private ILogService _logService;
        private IClientEntityService _clientEntityService;  

        public async Awaitable UpdatePrizes(CancellationToken token)
        {
            BoardData boardData = _gs.ch.Get<BoardData>();

            BoardPrizeSettings prizeSettings = _gameData.Get<BoardPrizeSettings>(_gs.ch);

            for (int i = 0; i < boardData.Length; i++)
            {
                ClientTile tileArt = _controller.GetTile(i);

                for (int s = 0; s < ExtraTileSlots.Max; s++)
                {
                    short prizeId = (s == ExtraTileSlots.Pass ? boardData.PassPrizes.Get(i) : boardData.LandPrizes.Get(i));
                    try
                    {
                        if (tileArt.Prizes[s] != prizeId)
                        {
                            tileArt.Prizes[s] = prizeId;

                            BoardPrize boardPrize = prizeSettings.Get(prizeId);

                            _clientEntityService.DestroyAllChildren(tileArt.PrizeAnchors[s]);
                            if (boardPrize != null)
                            {
                                _assetService.LoadAssetInto(tileArt.PrizeAnchors[s], AssetCategoryNames.BoardPrizes, boardPrize.Art, null, null, token);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logService.Exception(e, "Exception on load prize in tile: " + i + " " + boardData.Tiles.Get(i));
                    }
                }
            }


            await Task.CompletedTask;
        }
    }
}

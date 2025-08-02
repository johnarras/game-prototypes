using Genrpg.RequestServer.BoardGame.Boards.Services;
using Genrpg.RequestServer.BoardGame.Modes.Helpers;
using Genrpg.RequestServer.BoardGame.Modes.Services;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Prizes.Settings;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System.Reflection;

namespace Genrpg.RequestServer.BoardGame.Prizes.Services
{
    public interface IBoardPrizeService : IInjectable
    {
        Task UpdatePrizesForBoard(WebContext context, BoardData boardData = null);
    }
    public class BoardPrizeService : IBoardPrizeService
    {

        private IGameData _gameData = null;
        private IBoardModeService _boardModeService = null;
        private IBoardService _boardService = null;
        public async Task UpdatePrizesForBoard(WebContext context, BoardData boardData = null)
        {
            if (boardData == null)
            {
                boardData = await context.GetAsync<BoardData>();
            }

            IBoardModeHelper boardModeHelper = _boardModeService.GetBoardModeHelper(boardData.BoardModeId);

            if (boardModeHelper == null)
            {
                return;
            }

            BoardMode boardMode = _gameData.Get<BoardModeSettings>(context.user).Get(boardData.BoardModeId);

            if (boardMode == null)
            {
                return;
            }

            long[] allPathIndexes = boardData.GetAllPathIndexes();

            long maxPathIndex = BoardGameConstants.StartPathIndex;

            for (int i = 0; i < boardData.Length; i++)
            {
                long pathIndex = boardData.GetPathIndex(i);
                if (pathIndex > maxPathIndex)
                {
                    maxPathIndex = pathIndex;
                }
            }
            List<long> okTileTypes = _boardService.GetTileTypesWithPrizes(context);

            bool havePrizes = boardData.HasFlag(BoardFlags.EverAddedPrizes);

            BoardModePrizeRule globalRule = boardMode.PrizeRules.FirstOrDefault(x => x.PathIndex == 0);
            for (long p = BoardGameConstants.StartPathIndex; p <= maxPathIndex; p++)
            {
                BoardModePrizeRule rule = null;

                if (p == BoardGameConstants.StartPathIndex)
                {
                    rule = boardMode.PrizeRules.FirstOrDefault(x => x.PathIndex == BoardGameConstants.StartPathIndex);

                    if (rule == null)
                    {
                        rule = globalRule;
                    }
                }
                else
                {
                    rule = boardMode.PrizeRules.Where(x => x.PathIndex > BoardGameConstants.StartPathIndex && x.PathIndex <= p).OrderBy(x => x.PathIndex).LastOrDefault();

                    if (rule == null)
                    {
                        rule = globalRule;
                    }
                }

                if (rule == null)
                {
                    rule = boardMode.PrizeRules.FirstOrDefault(x => x.PathIndex == 0);
                }

                if (rule == null)
                {
                    continue;
                }

                if (havePrizes && rule.SpawnOnce)
                {
                    continue;
                }

                long totalTileQuantity = 0;

                List<long> okTileIndexes = new List<long>();

                for (int t = 0; t < boardData.Length; t++)
                {
                    if (allPathIndexes[t] != p)
                    {
                        continue;
                    }

                    if (!okTileTypes.Contains(boardData.Tiles.Get(t)))
                    {
                        continue;
                    }

                    okTileIndexes.Add(t);
                    totalTileQuantity++;
                }

                AddOneRewardTypeToPath(context, boardData.PassPrizes, rule.PassPercent, rule.PassPrizes, okTileIndexes, totalTileQuantity);
                AddOneRewardTypeToPath(context, boardData.LandPrizes, rule.LandPercent, rule.LandPrizes, okTileIndexes, totalTileQuantity);
            }




        }
        private void AddOneRewardTypeToPath(WebContext context,
            SmallIdShortCollection currArray,
            double spawnDensity,
            List<PrizeSpawn> prizeSpawns,
            List<long> okTileIndexes,
            long totalTileQuantity)
        {

            IReadOnlyList<BoardPrize> boardPrizes = _gameData.Get<BoardPrizeSettings>(context.user).GetData();

            List<long> openTileIndexes = new List<long>();

            int totalPrizes = 0;
            foreach (long tileIndex in okTileIndexes)
            {
                if (currArray.Get(tileIndex) == 0)
                {
                    openTileIndexes.Add(tileIndex);
                }
                else
                {
                    totalPrizes++;
                }
            }

            double prizeDensity = 1.0 * totalPrizes / totalTileQuantity;

            if (prizeDensity >= spawnDensity)
            {
                return;
            }

            int targetPrizeCount = (int)(spawnDensity * totalTileQuantity);

            long quantityToAdd = targetPrizeCount - totalPrizes;

            List<long> newPrizeIndexes = new List<long>();

            while (quantityToAdd > 0 && openTileIndexes.Count > 0)
            {
                long newIndex = openTileIndexes[context.rand.Next() % openTileIndexes.Count];
                openTileIndexes.Remove(newIndex);
                newPrizeIndexes.Add(newIndex);
                quantityToAdd--;
            }

            double weightSum = prizeSpawns.Sum(x => x.Weight);
            if (weightSum <= 0)
            {
                return;
            }

            for (int n = 0; n < newPrizeIndexes.Count; n++)
            {
                PrizeSpawn prize = RandomUtils.GetRandomElement(prizeSpawns, context.rand);

                BoardPrize bp = boardPrizes.FirstOrDefault(x => x.IdKey == prize.BoardPrizeId);

                if (prize != null && bp != null)
                {
                    currArray.Set(newPrizeIndexes[n], (short)prize.BoardPrizeId);
                }
            }
        }
    }
}

using Genrpg.RequestServer.BoardGame.Boards.Services;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.BoardGame.Upgrades.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Tiles.Settings;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Users.WebApi;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;

namespace Genrpg.RequestServer.BoardGame.BoardGen
{
    public interface IBoardGenService : IInjectable
    {
        Task<BoardData> GenerateBoard(WebContext context, BoardGenArgs genData = null);
        Task SetBoardTiles(WebContext context, BoardData boardData, int startIndex, int length, bool setOnlyRepeatable);
        Task<bool> TryLevelUpBoard(WebContext context, BoardData boardData, CoreUserData userData);
    }
    public class BoardGenService : IBoardGenService
    {
        protected IBoardService _boardService = null;
        protected IGameData _gameData = null;
        protected IUpgradeBoardService _upgradeService = null;

        public Type Key => GetType();

        public async Task<BoardData> GenerateBoard(WebContext context, BoardGenArgs args = null)
        {
            if (args == null)
            {
                // These are reasonable defaults for making a regular board for this user.
                args = new BoardGenArgs()
                {
                    BoardModeId = BoardModes.Primary
                };
            }

            BoardData boardData = await context.GetAsync<BoardData>();

            CoreUserData userData = await context.GetAsync<CoreUserData>();

            BoardGenSettings genSettings = _gameData.Get<BoardGenSettings>(context.user);
            
            boardData.Clear();
            boardData.OwnerId = args.OwnerId;
            boardData.Seed = args.Seed;
            boardData.BoardModeId = args.BoardModeId;
            boardData.Width = BoardGameConstants.MapWidth;
            boardData.Height = BoardGameConstants.MapHeight;
            boardData.StartUpgradeTier = _upgradeService.GetStartUpgradeTier(context.user, context.user.Level);
            boardData.UpgradeTierCount = _upgradeService.GetUpgradeTierCount(context.user, context.user.Level);

            TileTypeSettings settings = _gameData.Get<TileTypeSettings>(context.user);

            IReadOnlyList<TileType> tileTypes = settings.GetData();

            List<TileType> singleTiles = tileTypes.Where(x => (x.MinLevel == 1 || x.MinLevel <= context.user.Level) && x.Weight == 0).ToList();

            double desiredLength = genSettings.DistanceBetweenUniqueTiles * singleTiles.Count;

            if (desiredLength < 20)
            {
                desiredLength = 20;
            }

            double approxRadius = desiredLength / 9.5f;

            if (string.IsNullOrEmpty(boardData.OwnerId))
            {
                boardData.OwnerId = context.user.Id;
            }
            int[,] grid = new int[boardData.Width, boardData.Height];


            int xc = boardData.Width / 2;
            int zc = boardData.Height / 2;

            float radDelta = 0.2f;

            float xrad = MathUtils.FloatRange(approxRadius * (1-radDelta), approxRadius * (1+radDelta), context.rand);
            float zrad = MathUtils.FloatRange(approxRadius * (1-radDelta), approxRadius * (1+radDelta), context.rand);

            float xslope = MathUtils.FloatRange(-genSettings.SlopeMax,genSettings.SlopeMax, context.rand);

            int pointCount = MathUtils.IntRange(genSettings.MinPointCount, genSettings.MaxPointCount, context.rand);
            float angleGap = 360 / pointCount;
            float angleDelta = 0.0f;
            float startAngle = MathUtils.FloatRange(0, 360, context.rand);
            float angleRange = angleGap * angleDelta;

            List<MyPoint> startPoints = new List<MyPoint>();

            for (int i = 0; i < pointCount; i++)
            {
                float angle = startAngle + i * 360 / pointCount;

                float currAngle = angle + MathUtils.FloatRange(-angleRange, angleRange, context.rand);

                float radMultX = MathUtils.FloatRange(1 - genSettings.RadDelta, 1 + genSettings.RadDelta, context.rand);
                float radMultZ = MathUtils.FloatRange(1 - genSettings.RadDelta, 1 + genSettings.RadDelta, context.rand);

                float ax = (float)Math.Cos(currAngle * Math.PI / 180);
                float az = (float)Math.Sin(currAngle * Math.PI / 180);

                float x = xc + ax * xrad * radMultX;
                float z = zc + az * zrad * radMultZ;

                z += (xc - x) * xslope;

                int fx = (int)(MathUtils.Clamp(1, x, boardData.Width - 2));
                int fz = (int)(MathUtils.Clamp(1, z, boardData.Height - 2));

                startPoints.Add(new MyPoint(fx, fz));

            }

            if (context.rand.NextDouble() < 0.5f)
            {
                startPoints.Reverse();
            }

            int xmin = startPoints.Min(x=> x.X);
            int xmax = startPoints.Max(x => x.X);
            int ymin = startPoints.Min(x=> x.Y);
            int ymax = startPoints.Max(x => x.Y);

            int dx = xmax - xmin;
            int dy = ymax - ymin;

            int totalLength = (dx + dy) * 2;

            List<PointXZ> finalPoints = new List<PointXZ>();

            for (int p = 0; p < startPoints.Count; p++)
            {
                MyPoint start = startPoints[p];
                MyPoint end = startPoints[(p + 1) % startPoints.Count];

                int dsx = start.X - xc;
                int dsz = start.Y - zc;
                int dex = end.X - xc;
                int dez = end.Y - zc;

                float dist1 = dsx * dsx + dez * dez;
                float dist2 = dex * dex + dsz * dsz;


                bool xFirst = dist1 < dist2;

                if (context.rand.NextDouble() < 0.50f)
                {
                    xFirst = !xFirst;
                }

                int times = 0;
                if (xFirst)
                {
                    int xdelta = Math.Sign(end.X - start.X);
                    for (int x = start.X; x != end.X; x += xdelta)
                    {

                        if (++times >= 1000)
                        {
                            break;
                        }
                        boardData.SetIsOnPath(x, start.Y, true);
                    }
                    int ydelta = Math.Sign(end.Y - start.Y);
                    for (int y = start.Y; y != end.Y; y += ydelta)
                    {
                        if (++times >= 100)
                        {
                            break;
                        }
                        boardData.SetIsOnPath(end.X,y,true);
                    }
                }
                else
                {
                    int ydelta = Math.Sign(end.Y - start.Y);
                    for (int y = start.Y; y != end.Y; y += ydelta)
                    {
                        if (++times >= 100)
                        {
                            break;
                        }
                        boardData.SetIsOnPath(start.X, y, true);
                    }

                    int xdelta = Math.Sign(end.X - start.X);
                    for (int x = start.X; x != end.X; x += xdelta)
                    {
                        if (++times >= 100)
                        {
                            break;
                        }
                        boardData.SetIsOnPath(x, end.Y, true);
                    }
                }
            }


            int pathLength = 0;
            for (int x = 0; x < boardData.Width; x++)
            {
                for (int z = 0; z < boardData.Height; z++)
                {
                    if (boardData.IsOnPath(x,z))
                    {
                        pathLength++;
                    }
                }
            }

            boardData.Length = pathLength;

            bool isGoodSize = true;
            Console.WriteLine("Length: " + boardData.Length + " " + desiredLength);
            if (boardData.Length > desiredLength + singleTiles.Count*3/4)
            {
                args.SizeFailTimes++;
                isGoodSize = false;
            }

            if (boardData.Length < desiredLength - singleTiles.Count*3/4)
            {
                args.SizeFailTimes++;
                isGoodSize = false;
            }

            if (boardData.Length % 7 == 0)
            {
                isGoodSize = false;
            }

            if (!isGoodSize || !boardData.IsValid())
            {
                args.Seed = context.rand.Next() % 1000000000;
                await GenerateBoard(context, args);
            }
            else
            {
                await SetBoardTiles(context, boardData, 0, boardData.Length, false);
                // Get the actual tiles.
            }
            return boardData;
        }

        public async Task SetBoardTiles(WebContext context, BoardData boardData, int startIndex, int length, bool setOnlyRepeatable)
        {

            CoreUserData userData = await context.GetAsync<CoreUserData>();

            TileTypeSettings tileSettings = _gameData.Get<TileTypeSettings>(context.user);

            List<TileType> allTileTypes = tileSettings.GetData().ToList();

            List<TileType> multiTileTypes = allTileTypes.Where(x => x.Weight > 0).ToList();
            List<TileType> singleTileTypes = allTileTypes.Except(multiTileTypes).ToList();

            double multiTileWeightSum = multiTileTypes.Sum(x => x.Weight);

            boardData.Tiles.Set(0, TileTypes.TownHall);

            singleTileTypes = singleTileTypes.Where(x=>x.IdKey != TileTypes.TownHall).ToList();

            int delta = singleTileTypes.Count;

            int currSum = delta / 2;

            //long tileTypeId = multiTileTypes[context.rand.Next() % multiTileTypes.Count].IdKey;

            int lastSpecialTileIndex = 0;
            for (int i = 0; i < length; i++)
            {
                int index = (startIndex + i) % boardData.Length;

                TileType currTileAtIndex = tileSettings.Get(boardData.Tiles.Get(index));

                currSum += delta;
                if (currTileAtIndex != null && currTileAtIndex.Weight == 0)
                {
                    continue;
                }

                if (currSum < boardData.Length || singleTileTypes.Count < 1 || setOnlyRepeatable)
                {
                    double weightChosen = context.rand.NextDouble() * multiTileWeightSum;

                    foreach (TileType tileType in multiTileTypes)
                    {
                        weightChosen -= tileType.Weight;
                        if (weightChosen <= 0)
                        {
                            boardData.Tiles.Set(index, (short)tileType.IdKey);
                            //boardData.Tiles.Set(index, (short)tileTypeId);
                            break;
                        }
                    }

                    continue;
                }

                currSum -= boardData.Length;

                // Pick special tile.

                TileType singleType = singleTileTypes[context.rand.Next(singleTileTypes.Count)];

                singleTileTypes.Remove(singleType);

                boardData.Tiles.Set(index, (short)(singleType.IdKey));
                //boardData.Tiles.Set(index, (short)TileTypes.TownHall);
                lastSpecialTileIndex = i;

            }
            boardData.Tiles.Trim();
            await Task.CompletedTask;
        }

        public async Task<bool> TryLevelUpBoard(WebContext context, BoardData boardData, CoreUserData userData)
        {
            if (!_upgradeService.BoardIsComplete(context.user, boardData, userData))
            {
                return false;
            }

            context.user.Level++;
            context.Responses.AddResponse(new UpdateClientUserResponse() { Level = context.user.Level });

            BoardGenArgs args = new BoardGenArgs()
            {
                BoardModeId = BoardModes.Primary,
                OwnerId = context.user.Id,
                Seed = context.rand.Next(),
            };

            BoardData newBoardData = await GenerateBoard(context, args);

            if (newBoardData != null)
            {
                context.Responses.AddResponse(new NextBoardData() { NextBoard = newBoardData });
            }
            return true;
        }
    }
}

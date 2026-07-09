using Assets.Scripts.Crawler.MapGen.Helpers;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.MapGen.Constants;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.MapGen.DungeonGen.Helpers
{
    public class DungeonRandomWallsGenHelper : BaseDungeonGenHelper
    {
        public override long HelperKey => DungeonTypes.RandomWalls;

        public override async ValueTask<bool> GenerateLevel(CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs)
        {
            await Task.CompletedTask;
            double wallChance = RandUtils.FloatRange(genData.GenType.MinWallChance, genData.GenType.MaxWallChance, levelArgs.Rand);
            double doorChance = RandUtils.FloatRange(genData.GenType.MinDoorChance, genData.GenType.MaxDoorChance, levelArgs.Rand);
            for (int x = 0; x < levelArgs.Map.Width; x++)
            {
                for (int z = 0; z < levelArgs.Map.Height; z++)
                {
                    levelArgs.Map.Set(x, z, CellIndex.Terrain, genData.ZoneType.IdKey);
                    int index = levelArgs.Map.GetIndex(x, z);
                    int wallValue = 0;
                    if (levelArgs.Rand.NextDouble() < wallChance)
                    {
                        if (x == levelArgs.Map.Width - 1 && !levelArgs.Map.HasFlag(CrawlerMapFlags.IsLooping))
                        {
                            wallValue |= WallTypes.Wall << MapWallBits.EWallStart;
                        }
                        else if (levelArgs.Rand.NextDouble() > doorChance)
                        {
                            wallValue |= WallTypes.Wall << MapWallBits.EWallStart;
                        }
                        else
                        {
                            wallValue |= WallTypes.Door << MapWallBits.EWallStart;
                        }

                        if (z == levelArgs.Map.Height - 1 && !levelArgs.Map.HasFlag(CrawlerMapFlags.IsLooping))
                        {
                            wallValue |= WallTypes.Wall << MapWallBits.NWallStart;
                        }
                        else if (levelArgs.Rand.NextDouble() > doorChance)
                        {
                            wallValue |= (WallTypes.Wall << MapWallBits.NWallStart);
                        }
                        else
                        {
                            wallValue |= WallTypes.Door << MapWallBits.NWallStart;
                        }

                    }
                    else
                    {
                        if (x == levelArgs.Map.Width - 1 && !levelArgs.Map.HasFlag(CrawlerMapFlags.IsLooping))
                        {
                            wallValue |= WallTypes.Wall << MapWallBits.EWallStart;
                        }
                        if (z == levelArgs.Map.Height - 1 && !levelArgs.Map.HasFlag(CrawlerMapFlags.IsLooping))
                        {
                            wallValue |= WallTypes.Wall << MapWallBits.NWallStart;
                        }
                    }
                    levelArgs.Map.Set(x, z, CellIndex.Walls, wallValue);
                }
            }

            AddRandomRooms(genData, levelArgs);
            ConnectOpenCells(genData, levelArgs);

            double roomTimes = levelArgs.Map.Width * levelArgs.Map.Height / 200.0f;

            double roomremainder = roomTimes - (int)roomTimes;
            roomTimes = (int)roomTimes;
            if (levelArgs.Rand.NextDouble() < roomremainder)
            {
                roomTimes++;
            }

            int maxRoomSize = 6;
            for (int r = 0; r < roomTimes; r++)
            {
                int minx = RandUtils.IntRange(0, levelArgs.Map.Width - maxRoomSize - 1, levelArgs.Rand);
                int maxx = minx + RandUtils.IntRange(maxRoomSize / 2, maxRoomSize, levelArgs.Rand);

                int minz = RandUtils.IntRange(0, levelArgs.Map.Height - maxRoomSize - 1, levelArgs.Rand);
                int maxz = RandUtils.IntRange(maxRoomSize / 2, maxRoomSize, levelArgs.Rand);

                for (int x = minx; x < maxx; x++)
                {
                    for (int z = minz; z < maxz; z++)
                    {
                        levelArgs.Map.Set(x, z, CellIndex.Walls, 0);
                    }
                }
            }


            int exitEdgeDistance = 1;
            levelArgs.EnterX = RandUtils.IntRange(exitEdgeDistance, levelArgs.Map.Width - 1 - exitEdgeDistance, levelArgs.Rand);
            levelArgs.EnterZ = RandUtils.IntRange(exitEdgeDistance, levelArgs.Map.Height - 1 - exitEdgeDistance, levelArgs.Rand);

            do
            {
                levelArgs.ExitX = RandUtils.IntRange(exitEdgeDistance, levelArgs.Map.Width - 1 - exitEdgeDistance, levelArgs.Rand);
                levelArgs.ExitZ = RandUtils.IntRange(exitEdgeDistance, levelArgs.Map.Height - 1 - exitEdgeDistance, levelArgs.Rand);
            }
            while (levelArgs.EnterX == levelArgs.ExitX && levelArgs.EnterZ == levelArgs.ExitZ);

            List<Point2I> usedPoints = new List<Point2I>();
            usedPoints.Add(new Point2I(levelArgs.EnterX, levelArgs.EnterZ));
            usedPoints.Add(new Point2I(levelArgs.ExitX, levelArgs.ExitZ));

            for (int i = 0; i < 3; i++)
            {
                do
                {
                    int px = levelArgs.Rand.Next() % levelArgs.Map.Width;
                    int pz = levelArgs.Rand.Next() % levelArgs.Map.Height;

                    bool matchedExistingPoint = false;

                    foreach (Point2I pt in usedPoints)
                    {
                        if (pt.X == px && pt.Z == pz)
                        {
                            matchedExistingPoint = true;
                            break;
                        }
                    }

                    if (!matchedExistingPoint)
                    {
                        usedPoints.Add(new Point2I(px, pz));
                        break;
                    }
                }
                while (true);

            }
            return true;
        }

        private void AddRandomRooms(CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs)
        {
            int size = levelArgs.Map.Width * levelArgs.Map.Height;

            double roomCountFloat = size / 40.0;

            int roomCount = (int)roomCountFloat;
            if (levelArgs.Rand.NextDouble() < (roomCountFloat - roomCount))
            {
                roomCount++;
            }

            float minDiv = 8.0f;
            float maxDiv = 4.0f;

            float doorChance = 0.2f;

            double minWidth = levelArgs.Map.Width / minDiv;
            double maxWidth = levelArgs.Map.Width / maxDiv;

            double minHeight = levelArgs.Map.Height / minDiv; ;
            double maxHeight = levelArgs.Map.Height / maxDiv;

            for (int r = 0; r < roomCount; r++)
            {
                double widthFloat = Math.Min(RandUtils.FloatRange(minWidth, maxWidth, levelArgs.Rand), RandUtils.FloatRange(minWidth, maxWidth, levelArgs.Rand));
                double heightFloat = Math.Min(RandUtils.FloatRange(minHeight, maxHeight, levelArgs.Rand), RandUtils.FloatRange(minHeight, maxHeight, levelArgs.Rand));

                int width = (int)widthFloat;
                int height = (int)heightFloat;

                if (levelArgs.Rand.NextDouble() < (widthFloat - width))
                {
                    width++;
                }

                if (levelArgs.Rand.NextDouble() < (heightFloat - height))
                {
                    height++;
                }

                int xstart = RandUtils.IntRange(1, levelArgs.Map.Width - width - 2, levelArgs.Rand);
                int zstart = RandUtils.IntRange(1, levelArgs.Map.Height - height - 2, levelArgs.Rand);

                int xend = xstart + width;
                int zend = zstart + height;

                for (int x = xstart - 1; x <= xend; x++)
                {
                    for (int z = zstart - 1; z <= zend; z++)
                    {

                        if (levelArgs.RoomIds[x, z] == 0)
                        {
                            levelArgs.RoomIds[x, z] = (r + 1);
                        }
                        int northBits = levelArgs.Map.NorthWall(x, z);
                        int eastBits = levelArgs.Map.EastWall(x, z);

                        int walls = levelArgs.Map.Get(x, z, CellIndex.Walls);

                        if (x == xstart - 1 || x == xend)
                        {
                            if (levelArgs.Rand.NextDouble() < doorChance)
                            {
                                eastBits = WallTypes.Door;
                            }
                            else
                            {
                                eastBits = WallTypes.Wall;
                            }
                        }
                        else
                        {
                            eastBits = 0;
                        }
                        if (z == zstart - 1 || z == zend)
                        {
                            if (levelArgs.Rand.NextDouble() < doorChance)
                            {
                                northBits = WallTypes.Door;
                            }
                            else
                            {
                                northBits = WallTypes.Wall;
                            }
                        }
                        else
                        {
                            northBits = 0;
                        }

                        int finalBits = northBits << MapWallBits.NWallStart |
                            eastBits << MapWallBits.EWallStart;

                        levelArgs.Map.Set(x, z, CellIndex.Walls, finalBits);
                    }
                }
            }
        }

        protected void ConnectOpenCells(CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs)
        {

            bool[,] openCell = new bool[levelArgs.Map.Width, levelArgs.Map.Height];

            for (int x = 0; x < levelArgs.Map.Width; x++)
            {
                for (int z = 0; z < levelArgs.Map.Height; z++)
                {
                    openCell[x, z] = levelArgs.Map.Get(x, z, CellIndex.Terrain) >= 0;
                }
            }

            while (true)
            {
                bool hadDisconnectedCell = false;

                bool[,] connectedCells = new bool[levelArgs.Map.Width, levelArgs.Map.Height];

                Queue<Point2I> cellsToCheck = new Queue<Point2I>();

                cellsToCheck.Enqueue(new Point2I(levelArgs.Map.Width / 2, levelArgs.Map.Height / 2));

                while (cellsToCheck.Count > 0)
                {
                    Point2I currentCell = cellsToCheck.Dequeue();

                    int x = currentCell.X;
                    int z = currentCell.Z;

                    connectedCells[x, z] = true;

                    // If x on right or levelArgs.Map loops, see if there's a disconnected cell to east.
                    if (x < levelArgs.Map.Width - 1 || levelArgs.Map.HasFlag(CrawlerMapFlags.IsLooping))
                    {
                        int nx = (x + 1) % levelArgs.Map.Width;
                        if (!connectedCells[nx, z] && !WallTypes.IsBlockingType(levelArgs.Map.EastWall(x, z)))
                        {
                            connectedCells[nx, z] = true;
                            cellsToCheck.Enqueue(new Point2I(nx, z));
                        }
                    }
                    if (x > 0 || levelArgs.Map.HasFlag(CrawlerMapFlags.IsLooping))
                    {
                        int nx = (x - 1 + levelArgs.Map.Width) % levelArgs.Map.Width;
                        if (!connectedCells[nx, z] && !WallTypes.IsBlockingType(levelArgs.Map.EastWall(nx, z)))
                        {
                            connectedCells[nx, z] = true;
                            cellsToCheck.Enqueue(new Point2I(nx, z));
                        }
                    }

                    if (z < levelArgs.Map.Height - 1 || levelArgs.Map.HasFlag(CrawlerMapFlags.IsLooping))
                    {
                        int nz = (z + 1) % levelArgs.Map.Height;
                        if (!connectedCells[x, nz] && !WallTypes.IsBlockingType(levelArgs.Map.NorthWall(x, z)))
                        {
                            connectedCells[x, nz] = true;
                            cellsToCheck.Enqueue(new Point2I(x, nz));
                        }
                    }
                    if (z > 0 || levelArgs.Map.HasFlag(CrawlerMapFlags.IsLooping))
                    {
                        int nz = (z - 1 + levelArgs.Map.Height) % levelArgs.Map.Height;
                        if (!connectedCells[x, nz] && !WallTypes.IsBlockingType(levelArgs.Map.NorthWall(x, nz)))
                        {
                            connectedCells[x, nz] = true;
                            cellsToCheck.Enqueue(new Point2I(x, nz));
                        }
                    }
                }

                for (int x = 0; x < levelArgs.Map.Width; x++)
                {
                    for (int z = 0; z < levelArgs.Map.Height; z++)
                    {
                        if (openCell[x, z] && !connectedCells[x, z])
                        {
                            hadDisconnectedCell = true;

                            if (levelArgs.Rand.NextDouble() > 0.1f)
                            {
                                continue;
                            }

                            long bits = levelArgs.Map.Get(x, z, CellIndex.Walls);

                            if (x < levelArgs.Map.Width - 1 || genData.Looping)
                            {
                                bits &= ~(WallTypes.Wall << MapWallBits.EWallStart);
                            }
                            if (z < levelArgs.Map.Height - 1 || genData.Looping)
                            {
                                bits &= ~(WallTypes.Wall << MapWallBits.NWallStart);
                            }

                            levelArgs.Map.Set(x, z, CellIndex.Walls, (byte)bits);

                            if (x > 0 || genData.Looping)
                            {
                                int nx = (x + levelArgs.Map.Width - 1) % levelArgs.Map.Width;

                                long ebits = levelArgs.Map.Get(nx, z, CellIndex.Walls);

                                ebits &= ~(WallTypes.Wall << MapWallBits.EWallStart);
                                levelArgs.Map.Set(nx, z, CellIndex.Walls, (byte)ebits);
                            }
                            if (z > 0 || genData.Looping)
                            {
                                int nz = (z + levelArgs.Map.Height - 1) % levelArgs.Map.Height;
                                long nbits = levelArgs.Map.Get(x, nz, CellIndex.Walls);
                                nbits &= ~(WallTypes.Wall << MapWallBits.NWallStart);
                                levelArgs.Map.Set(x, nz, CellIndex.Walls, (byte)nbits);
                            }

                        }
                    }
                }

                if (!hadDisconnectedCell)
                {
                    break;
                }
            }
        }


    }
}

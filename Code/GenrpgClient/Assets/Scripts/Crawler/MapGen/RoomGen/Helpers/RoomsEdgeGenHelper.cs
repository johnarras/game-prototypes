using Assets.Scripts.Crawler.MapGen.Helpers;
using Assets.Scripts.Crawler.MapGen.RoomGen.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.MapGen.Constants;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Zones.Constants;
using System;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.MapGen.RoomGen.Helpers
{

    public class RoomsEdgeGenHelper : BaseEdgeGenHelper
    {
        public override long HelperKey => RoomEdgeTypes.Rooms;


        protected override int GetRowLength(RoomEdgeGenArgs edgeArgs, int pindex, IRandom rand)
        {
            return edgeArgs.GetMaxLength();
        }

        public override async ValueTask GenerateEdge(RoomEdgeGenArgs edgeArgs, CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs)
        {
            EdgeStartEndPoints startEndPoints = edgeArgs.Corners.GetStartEndPoints(edgeArgs.DX, edgeArgs.DZ);

            Point2I start = startEndPoints.Start;
            Point2I end = startEndPoints.End;

            int wallXLen = end.X - start.X;
            int wallZLen = end.Z - start.Z;

            int wallDx = Math.Sign(wallXLen);
            int wallDz = Math.Sign(wallZLen);

            int len = Math.Abs(wallXLen) + Math.Abs(wallZLen);

            int roomCount = 0;

            int xOffset = 1;
            int zOffset = 1;

            int minSize = 1;
            int maxSize = 3;

            double missingChance = RandUtils.FloatRange(edgeArgs.EdgeType.MinMissingChance, edgeArgs.EdgeType.MaxMissingChance, levelArgs.Rand);

            CrawlerMap map = levelArgs.Map;

            int actualLLCornerX = start.X;
            int actualLLCornerZ = start.Z;

            int distanceGone = 0;
            while (distanceGone < len)
            {
                int width = RandUtils.IntRange(minSize, maxSize, levelArgs.Rand);
                int height = RandUtils.IntRange(minSize, maxSize, levelArgs.Rand);
                int roomStartX = actualLLCornerX + wallDx * (distanceGone - 1);
                int roomStartZ = actualLLCornerZ + wallDz * (distanceGone - 1);
                bool canMakeRoom = true;
                for (int w = xOffset; w < width + xOffset; w++)
                {
                    for (int h = zOffset; h < height + zOffset; h++)
                    {
                        int x = roomStartX + w * wallDx + h * edgeArgs.DX;
                        int z = roomStartZ + w * wallDz + h * edgeArgs.DZ;

                        if (x < 1 || z < 1 || x >= map.Width - 1 || z >= map.Height - 1)
                        {
                            canMakeRoom = false;
                            break;
                        }

                        if (levelArgs.AdjacentRoomIds[x, z] != 0 && levelArgs.AdjacentRoomIds[x, z] != edgeArgs.RoomId)
                        {
                            canMakeRoom = false;
                            break;
                        }
                    }
                }

                if (!canMakeRoom)
                {
                    break;
                }

                for (int w = xOffset - 1; w <= width + xOffset; w++)
                {
                    for (int h = zOffset - 1; h <= height + zOffset; h++)
                    {
                        int x = roomStartX + w * wallDx + h * edgeArgs.DX;
                        int z = roomStartZ + w * wallDz + h * edgeArgs.DZ;

                        levelArgs.AdjacentRoomIds[x, z] = edgeArgs.RoomId;
                    }
                }

                int doorIndex = 1 + levelArgs.Rand.Next() % width;

                for (int w = xOffset; w < width + xOffset; w++)
                {
                    for (int h = zOffset; h < height + zOffset; h++)
                    {
                        int x = roomStartX + w * wallDx + h * edgeArgs.DX;
                        int z = roomStartZ + w * wallDz + h * edgeArgs.DZ;


                        levelArgs.RoomIds[x, z] = edgeArgs.RoomId;
                        map.Set(x, z, CellIndex.Terrain, ZoneTypes.Badlands);

                        levelArgs.SetFlag(x, z, DungeonLevelFlags.EdgeCell);
                        if (w == xOffset)
                        {
                            _mapGenService.SetWallBitsFromDeltas(map, x, z, -wallDx, -wallDz, WallTypes.Wall);
                        }
                        else if (w == width - 1 + xOffset)
                        {
                            _mapGenService.SetWallBitsFromDeltas(map, x, z, wallDx, wallDz, WallTypes.Wall);
                        }

                        if (h == zOffset)
                        {
                            _mapGenService.SetWallBitsFromDeltas(map, x, z, -edgeArgs.DX, -edgeArgs.DZ, w == doorIndex ? WallTypes.Door : WallTypes.Wall);

                        }
                        else if (h == height - 1 + zOffset)
                        {
                            levelArgs.SetFlag(x, z, DungeonLevelFlags.EdgeEndCell);
                            _mapGenService.SetWallBitsFromDeltas(map, x, z, edgeArgs.DX, edgeArgs.DZ, WallTypes.Wall);
                        }
                    }
                }

                distanceGone += width;

                if (levelArgs.Rand.NextDouble() < missingChance)
                {
                    distanceGone++;
                }
            }
        }
    }
}

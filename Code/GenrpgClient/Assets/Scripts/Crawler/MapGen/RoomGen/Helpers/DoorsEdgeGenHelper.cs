using OxDb.Client.Crawler.MapGen.Helpers;
using OxDb.Client.Crawler.MapGen.RoomGen.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.MapGen.Constants;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Zones.Constants;
using System;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.MapGen.RoomGen.Helpers
{

    public class DoorsEdgeGenHelper : BaseEdgeGenHelper
    {
        public override long HelperKey => RoomEdgeTypes.Doors;


        protected override int GetRowLength(RoomEdgeGenArgs edgeArgs, int pindex, IRandom rand)
        {
            return edgeArgs.GetMaxLength();
        }

        public override async ValueTask GenerateEdge(RoomEdgeGenArgs edgeArgs, CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs)
        {

            await Task.CompletedTask;
            EdgeStartEndPoints startEndPoints = edgeArgs.Corners.GetStartEndPoints(edgeArgs.DX, edgeArgs.DZ);

            Point2I start = startEndPoints.Start;
            Point2I end = startEndPoints.End;

            int wallXLen = end.X - start.X;
            int wallZLen = end.Z - start.Z;

            int wallDx = Math.Sign(wallXLen);
            int wallDz = Math.Sign(wallZLen);

            int len = Math.Abs(wallXLen) + Math.Abs(wallZLen);

            int middleWidth = 1;
            if (len >= 5 && levelArgs.Rand.NextDouble() < 0.1f)
            {
                middleWidth++;
            }

            int totalWidth = middleWidth + 2;

            int maxStartPos = len - totalWidth;

            int startPos = 0;

            if (maxStartPos > 1)
            {
                startPos = levelArgs.Rand.Next() % (maxStartPos + 1);
            }

            int endPos = startPos + totalWidth - 1;


            int xOffset = 1;
            int zOffset = 1;
            int actualLLCornerX = start.X;
            int actualLLCornerZ = start.Z;

            int roomStartX = actualLLCornerX - wallDx;
            int roomStartZ = actualLLCornerZ - wallDz;
            CrawlerMap map = levelArgs.Map;

            int maxHeightPossible = RandUtils.IntRange(3, 5, levelArgs.Rand);

            if (levelArgs.Rand.NextDouble() < 0.1f)
            {
                maxHeightPossible += RandUtils.IntRange(2, 5, levelArgs.Rand);
            }

            int maxHeightAchieved = 0;

            for (int h = zOffset; h < maxHeightPossible + zOffset; h++)
            {
                bool canAddRoomRow = true;
                for (int w = startPos + xOffset; w <= endPos + xOffset; w++)
                {

                    int x = roomStartX + w * wallDx + h * edgeArgs.DX;
                    int z = roomStartZ + w * wallDz + h * edgeArgs.DZ;

                    if (x < 1 || z < 1 || x >= map.Width - 1 || z >= map.Height - 1)
                    {
                        canAddRoomRow = false;
                        break;
                    }


                    if (levelArgs.AdjacentRoomIds[x, z] != 0 && levelArgs.AdjacentRoomIds[x, z] != edgeArgs.RoomId)
                    {
                        canAddRoomRow = false;
                        break;
                    }
                }

                for (int w = startPos + xOffset; w <= endPos + xOffset; w++)
                {

                    int x = roomStartX + w * wallDx + h * edgeArgs.DX;
                    int z = roomStartZ + w * wallDz + h * edgeArgs.DZ;

                    if (x < 1 || x >= map.Width - 1 || z < 1 || z >= map.Height - 1)
                    {
                        continue;
                    }

                    if (w != startPos + xOffset && w != endPos + xOffset)
                    {
                        levelArgs.RoomIds[x, z] = edgeArgs.RoomId;
                        map.Set(x, z, CellIndex.Terrain, genData.ZoneType.IdKey);
                        if (!canAddRoomRow && h > zOffset)
                        {
                            _mapGenService.SetWallBitsFromDeltas(map, x, z, -edgeArgs.DX, -edgeArgs.DZ, WallTypes.Wall);
                        }
                        else if (h == zOffset)
                        {
                            _mapGenService.SetWallBitsFromDeltas(map, x, z, -edgeArgs.DX, -edgeArgs.DZ, WallTypes.Door);
                        }
                    }
                    else
                    {
                        if (w == startPos + xOffset)
                        {
                            _mapGenService.AddRoomWithDoor(levelArgs, x, z, MapDirUtils.GetDirFromDeltas(wallDx, wallDz).Dir, genData.ZoneType.IdKey);
                        }
                        else if (w == endPos + xOffset)
                        {
                            _mapGenService.AddRoomWithDoor(levelArgs, x, z, MapDirUtils.GetDirFromDeltas(-wallDx, -wallDz).Dir, genData.ZoneType.IdKey);
                        }
                    }
                }

                if (!canAddRoomRow)
                {
                    break;
                }
                maxHeightAchieved = h;
            }


            for (int h = zOffset; h < maxHeightAchieved + zOffset; h++)
            {
                for (int w = startPos + xOffset; w <= endPos + xOffset; w++)
                {

                    int x = roomStartX + w * wallDx + h * edgeArgs.DX;
                    int z = roomStartZ + w * wallDz + h * edgeArgs.DZ;

                    if (x < 1 || z < 1 || x >= map.Width - 1 || z >= map.Height - 1)
                    {
                        continue;
                    }
                    levelArgs.AdjacentRoomIds[x, z] = edgeArgs.RoomId;
                }
            }
        }
    }
}

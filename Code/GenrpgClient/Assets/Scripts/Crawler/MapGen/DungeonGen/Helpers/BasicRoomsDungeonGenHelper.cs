using Assets.Scripts.Crawler.MapGen.Helpers;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.MapGen.Constants;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.ProcGen.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.MapGen.DungeonGen.Helpers
{
    public class BasicRoomsDungeonGenHelper : BaseDungeonGenHelper
    {
        public override long HelperKey => DungeonTypes.BasicRooms;

        public override async ValueTask<bool> GenerateLevel(CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs)
        {
            await Task.CompletedTask;
            // Add rooms first.

            int roomCount = (int)(Math.Sqrt(levelArgs.Map.Width * levelArgs.Map.Height) * RandUtils.FloatRange(genData.GenType.MinCorridorDensity, genData.GenType.MaxCorridorDensity, levelArgs.Rand));

            if (roomCount < 1)
            {
                roomCount++;
            }

            int edgeSize = 1;
            SamplingData sd = new SamplingData()
            {
                MaxAttemptsPerItem = 20,
                Count = roomCount,
                MinSeparation = 3,
                MinX = edgeSize,
                MinZ = edgeSize,
                MaxX = levelArgs.Map.Width - 1 - edgeSize,
                MaxZ = levelArgs.Map.Height - 1 - edgeSize,
            };

            SamplingResult result = _samplingService.PlanePoissonSample(sd);

            if (result.Points.Count < 1)
            {
                result.Points.Add(new SampledPoint(levelArgs.Map.Width / 2, levelArgs.Map.Height / 2, 1));
            }

            List<SampledPoint> roomCenters = result.Points;

            bool[,] clearCells = new bool[levelArgs.Map.Width, levelArgs.Map.Height];

            List<Point2I> newRoomCenters = new List<Point2I>();

            for (int i = 0; i < roomCenters.Count; i++)
            {
                int minx = roomCenters[i].X;
                int maxx = roomCenters[i].X;
                int minz = roomCenters[i].Z;
                int maxz = roomCenters[i].Z;

                int[] roomSizes = new int[2] { 2, 2 };

                float[] roomSizeIncreaseChances = { 0.5f, 0.3f, 0.1f, 0.1f };

                for (int ii = 0; ii < roomSizes.Length; ii++)
                {
                    for (int r = 0; r < roomSizeIncreaseChances.Length; r++)
                    {
                        if (levelArgs.Rand.NextDouble() < roomSizeIncreaseChances[ii])
                        {
                            roomSizes[ii]++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                bool failedMinX = false;
                bool failedMaxX = false;
                while (maxx - minx + 1 < roomSizes[0] && (!failedMinX || !failedMaxX))
                {
                    if (levelArgs.Rand.NextDouble() < 0.5f)
                    {
                        if (!_mapGenService.RoomAreaIsBlank(levelArgs.RoomIds, minx - 1, maxx, minz, maxz))
                        {
                            failedMinX = true;
                        }
                        else
                        {
                            minx--;
                        }
                    }
                    else
                    {
                        if (!_mapGenService.RoomAreaIsBlank(levelArgs.RoomIds, minx, maxx + 1, minz, maxz))
                        {
                            failedMaxX = true;
                        }
                        else
                        {
                            maxx++;
                        }
                    }
                }

                bool failedMinZ = false;
                bool failedMaxZ = false;

                while (maxz - minz + 1 < roomSizes[1] && (!failedMinZ || !failedMaxZ))
                {
                    if (levelArgs.Rand.NextDouble() < 0.5f)
                    {
                        if (!_mapGenService.RoomAreaIsBlank(levelArgs.RoomIds, minx, maxx, minz - 1, maxz))
                        {
                            failedMinZ = true;
                        }
                        else
                        {
                            minz--;
                        }
                    }
                    else
                    {
                        if (!_mapGenService.RoomAreaIsBlank(levelArgs.RoomIds, minx, maxx, minz, maxz + 1))
                        {
                            failedMaxZ = true;
                        }
                        else
                        {
                            maxz++;
                        }
                    }
                }


                minx = MathUtil.Clamp(edgeSize, minx, levelArgs.Map.Width - edgeSize - 1);
                maxx = MathUtil.Clamp(edgeSize, maxx, levelArgs.Map.Width - edgeSize - 1);
                minz = MathUtil.Clamp(edgeSize, minz, levelArgs.Map.Height - edgeSize - 1);
                maxz = MathUtil.Clamp(edgeSize, maxz, levelArgs.Map.Height - edgeSize - 1);


                for (int x = minx; x <= maxx; x++)
                {
                    for (int z = minz; z <= maxz; z++)
                    {
                        clearCells[x, z] = true;
                        levelArgs.Map.AddBits(x, z, CellIndex.Walls, 1 << MapWallBits.IsRoomBitOffset);
                        levelArgs.RoomIds[x, z] = i + 1;

                    }
                }

                for (int x = minx - 1; x <= maxx + 1; x++)
                {
                    if (x < 0 || x >= levelArgs.Map.Width)
                    {
                        continue;
                    }
                    for (int z = minz - 1; z <= maxz + 1; z++)
                    {
                        if (z < 0 || z >= levelArgs.Map.Height)
                        {
                            continue;
                        }

                        levelArgs.OverlappedRoomIds.SetBitIndex(levelArgs.AdjacentRoomIds[x, z]);
                    }
                }

                int midx = (minx + maxx) / 2;
                int midz = (minz + maxz) / 2;

                if ((minx + maxx) % 2 != 0 && levelArgs.Rand.NextDouble() < 0.5f)
                {
                    midx++;
                }
                if ((minz + maxz) % 2 != 0 && levelArgs.Rand.NextDouble() < 0.5f)
                {
                    midz++;
                }

                newRoomCenters.Add(new Point2I(midx, midz));
            }

            List<ConnectPointData> connectPoints = new List<ConnectPointData>();

            for (int p = 0; p < newRoomCenters.Count; p++)
            {
                Point2I pt = newRoomCenters[p];
                connectPoints.Add(new ConnectPointData()
                {
                    X = pt.X,
                    Z = pt.Z,
                    Id = p + 1,
                    MaxConnections = 0,
                    MinDistToOther = 1000000,
                });
            }

            connectPoints = connectPoints.OrderBy(x => Guid.NewGuid()).ToList();

            List<ConnectedPairData> newPaths = _lineGenService.ConnectPoints(connectPoints, levelArgs.Rand, 0.6f);

            foreach (ConnectPointData pt in connectPoints)
            {
                if (!newPaths.FastAny(x => (x.Point1.X == pt.X && x.Point1.Z == pt.Z) ||
                (x.Point2.X == pt.X && x.Point2.Z == pt.Z)))
                {
                    _logService.Info("Missing point: " + pt.X + " " + pt.Z);
                }
            }

            foreach (ConnectedPairData cpd in newPaths)
            {
                List<Point2I> newLine = _lineGenService.GridConnect((int)cpd.Point1.X, (int)cpd.Point1.Z,
                    (int)cpd.Point2.X, (int)cpd.Point2.Z, levelArgs.Rand.NextDouble() < 0.5f);


                foreach (Point2I p in newLine)
                {
                    if (p.X >= 0 && p.X < levelArgs.Map.Width && p.Z >= 0 && p.Z < levelArgs.Map.Height)
                    {
                        clearCells[p.X, p.Z] = true;
                    }
                }
            }

            for (int x = 0; x < levelArgs.Map.Width; x++)
            {
                for (int z = 0; z < levelArgs.Map.Height; z++)
                {

                    if (clearCells[x, z])
                    {
                        levelArgs.Map.Set(x, z, CellIndex.Terrain, genData.ZoneType.IdKey);
                    }

                    int wallValue = 0;
                    int leftx = (x + levelArgs.Map.Width - 1) % levelArgs.Map.Width;
                    int rightx = (x + 1) % levelArgs.Map.Width;
                    int upz = (z + 1) % levelArgs.Map.Height;
                    int downz = (z + levelArgs.Map.Height - 1) % levelArgs.Map.Height;

                    if (clearCells[x, z])
                    {
                        wallValue = levelArgs.Map.Get(x, z, CellIndex.Walls);
                        if (!clearCells[rightx, z])
                        {
                            wallValue |= (WallTypes.Wall << MapWallBits.EWallStart);
                        }
                        if (!clearCells[x, upz])
                        {
                            wallValue |= (WallTypes.Wall << MapWallBits.NWallStart);
                        }
                        levelArgs.Map.AddBits(x, z, CellIndex.Walls, wallValue);

                        if (!clearCells[leftx, z])
                        {
                            byte currWallValue = levelArgs.Map.Get(leftx, z, CellIndex.Walls);
                            currWallValue |= (WallTypes.Wall << MapWallBits.EWallStart);
                            levelArgs.Map.AddBits(leftx, z, CellIndex.Walls, currWallValue);
                        }
                        if (!clearCells[x, downz])
                        {
                            byte currWallValue = levelArgs.Map.Get(x, downz, CellIndex.Walls);
                            currWallValue |= (WallTypes.Wall << MapWallBits.NWallStart);
                            levelArgs.Map.AddBits(x, downz, CellIndex.Walls, currWallValue);
                        }
                    }
                }
            }


            _mapGenService.SetEntranceAndExitPoints(levelArgs.Map, levelArgs);

            return true;
        }


    }
}

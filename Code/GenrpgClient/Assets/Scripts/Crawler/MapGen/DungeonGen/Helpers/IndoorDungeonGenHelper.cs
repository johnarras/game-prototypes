using OxDb.Client.Crawler.MapGen.Helpers;
using OxDb.Client.Crawler.MapGen.RoomGen.Helpers;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.MapGen.Constants;
using OxDb.SharedGame.Crawler.MapGen.Settings;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.ProcGen.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.MapGen.DungeonGen.Helpers
{
    public class IndoorDungeonGenHelper : BaseDungeonGenHelper
    {
        public override long HelperKey => DungeonTypes.Indoors;

        public override async ValueTask<bool> GenerateLevel(CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs)
        {

            RoomGenSettings roomGenSettings = _gameData.Get<RoomGenSettings>(_gs.ch);

            RoomEdgeTypeSettings edgeTypeSettings = _gameData.Get<RoomEdgeTypeSettings>(_gs.ch);

            // Add rooms first.

            int minSeparation = roomGenSettings.MaxSize;

            int wantedRoomCount = (levelArgs.Map.Width * levelArgs.Map.Height / (minSeparation * minSeparation)) / 2;

            if (wantedRoomCount < 1)
            {
                wantedRoomCount++;
            }

            int edgeSize = roomGenSettings.MaxSize / 2;
            SamplingData sd = new SamplingData()
            {
                MaxAttemptsPerItem = 2,
                Count = wantedRoomCount * 2,
                MinSeparation = minSeparation,
                MinX = edgeSize,
                MinZ = edgeSize,
                MaxX = levelArgs.Map.Width - 1 - edgeSize,
                MaxZ = levelArgs.Map.Height - 1 - edgeSize,
                CreateIndexGrid = true,
            };

            SamplingResult result = _samplingService.PlanePoissonSample(sd);

            if (result.Points.Count < 1)
            {
                result.Points.Add(new SampledPoint(levelArgs.Map.Width / 2, levelArgs.Map.Height / 2, 1));
            }

            List<SampledPoint> startPoints = new List<SampledPoint>(result.Points);

            List<SampledPoint> points = result.Points.OrderBy(x => x.DistanceFromCenter).ToList();

            foreach (SampledPoint pt in points)
            {
                levelArgs.RoomIds[pt.X, pt.Z] = pt.Index;
            }

            int roomsPlaced = 0;

            List<SampledPoint> placedPoints = new List<SampledPoint>();

            do
            {
                SampledPoint nextRoomCenter = null;
                if (roomsPlaced == 0)
                {
                    nextRoomCenter = points[0];
                    points.Remove(nextRoomCenter);
                }
                else
                {
                    while (points.Count > 0)
                    {
                        if (levelArgs.Rand.NextDouble() < roomGenSettings.UseRoomChance)
                        {
                            nextRoomCenter = points[0];
                            points.Remove(nextRoomCenter);
                            break;
                        }
                        else
                        {
                            points.RemoveAt(0);
                        }
                    }
                }

                if (nextRoomCenter == null)
                {
                    break;
                }

                placedPoints.Add(nextRoomCenter);

                await _roomGenService.GenerateRoom(nextRoomCenter, genData, levelArgs);


#if UNITY_EDITOR
                if (BaseEdgeGenHelper.ForcedRoomEdgeType > 0)
                {
                    break;
                }
#endif

                roomsPlaced++;
                List<SampledPoint> overlappedPoints = new List<SampledPoint>();

                foreach (SampledPoint point in points)
                {
                    if (levelArgs.OverlappedRoomIds.HasBitIndex(point.Index))
                    {
                        overlappedPoints.Add(point);
                    }
                }

                points = points.Except(overlappedPoints).ToList();
            }
            while (points.Count > 0 && roomsPlaced < wantedRoomCount);

            List<ConnectPointData> centerPoints = new List<ConnectPointData>();


            int centerId = 0;
            foreach (Point2I loc in placedPoints)
            {
                ConnectPointData connectionData = new ConnectPointData()
                {
                    Id = ++centerId,
                    X = loc.X,
                    Z = loc.Z,
                    Data = loc,
                    MaxConnections = 0,
                };
                centerPoints.Add(connectionData);
            }


            List<ConnectedPairData> roadsToMake = _lineGenService.ConnectPoints(centerPoints, levelArgs.Rand, 0.9f);

            foreach (ConnectedPairData pairData in roadsToMake)
            {
                _mapGenService.ConnectPairOfPoints(levelArgs.Map, pairData, 2, new List<long>(), genData.ZoneType.IdKey, levelArgs.Rand);
            }

            _mapGenService.AddSmallRoomsAndBlankSpaces(levelArgs);

            _mapGenService.RemoveDisconnectedComponents(levelArgs.Map);

            _mapGenService.SetDungeonEntranceAndExitPoints(levelArgs.Map, levelArgs);

            return true;
        }
    }
}
using OxDb.Client.Crawler.Maps.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Buildings.Constants;
using OxDb.SharedGame.Buildings.Settings;
using OxDb.SharedGame.Crawler.MapGen.Entities;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Maps.Settings;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Quests.Settings;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.ProcGen.Entities;
using OxDb.SharedGame.Zones.Constants;
using OxDb.SharedGame.Zones.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.MapGen.Helpers
{
    public class CityCrawlerMapGenHelper : BaseCrawlerMapGenHelper
    {
        public override long HelperKey => CrawlerMapTypes.City;

        public override async Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData, CancellationToken token)
        {
            await Task.CompletedTask;
            MyRandom rand = new MyRandom(genData.World.Seed / 2 + genData.World.GetMaxMapId() / 7 + 13);

            IReadOnlyList<ZoneType> allZoneTypes = _gameData.Get<ZoneTypeSettings>(null).GetData();

            long cityZoneTypeId = allZoneTypes.FirstOrDefault(x => x.Name == "City")?.IdKey ?? 1;
            long roadZoneTypeId = allZoneTypes.FirstOrDefault(x => x.Name == "Road")?.IdKey ?? 1;
            long fillerZoneTypeId = ZoneTypes.Field;
            CrawlerMapGenType genType = genData.GenType;

            int mapEdgeDistance = 1;

            int width = (int)RandUtils.LongRange(genType.MinWidth, genType.MaxWidth, rand);
            int height = (int)RandUtils.LongRange(genType.MinHeight, genType.MaxHeight, rand);
            CrawlerMap map = _worldService.CreateMap(genData, width, height);

            map.Name = _zoneGenService.GenerateZoneName(genData.ZoneType.IdKey, rand.Next(), true);


            int edgeSize = 1;

            int gateX = -1;
            int gateZ = -1;

            ZoneEdge edge = new ZoneEdge();


            if (_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                if (rand.NextDouble() < 0.5f)
                {
                    if (rand.NextDouble() < 0.5f)
                    {
                        gateX = edgeSize;
                        edge.X = 0;
                        edge.DX = -1;
                    }
                    else
                    {
                        gateX = map.Width - 1 - edgeSize;
                        edge.X = map.Width - 1;
                        edge.DX = 1;
                    }

                   
                    gateZ = RandUtils.IntRange(height / 3, height * 2 / 3, rand);
                    edge.Z = gateZ;
                }
                else
                {
                    if (rand.NextDouble() < 0.5f)
                    {
                        gateZ = edgeSize;
                        edge.Z = 0;
                        edge.DZ = -1;
                    }
                    else
                    {
                        gateZ = map.Height - 1 - edgeSize;
                        edge.Z = map.Height - 1;
                        edge.DZ = 1;
                    }
                    gateX = RandUtils.IntRange(width / 3, width * 2 / 3, rand);
                    edge.X = gateX;
                }
            }

            map.EdgePoints.Add(edge);

            float minSeparation = RandUtils.FloatRange(2.5f, 3.5f, rand);

            int midWidth = width - edgeSize * 2;
            int midHeight = height - edgeSize * 2;

            int roomCount = (int)(midWidth * midHeight / (minSeparation * minSeparation) * 1.2f);

            SamplingData sd = new SamplingData()
            {
                MaxAttemptsPerItem = 20,
                Count = roomCount,
                MinSeparation = 3,
                MinX = edgeSize + 1,
                MinZ = edgeSize + 1,
                MaxX = map.Width - 1 - edgeSize - 1,
                MaxZ = map.Height - 1 - edgeSize - 1,
            };

            SamplingResult sampleResult = _samplingService.PlanePoissonSample(sd);

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    map.Set(x, z, CellIndex.Terrain, fillerZoneTypeId);
                }
            }

            List<SampledPoint> points = new List<SampledPoint>(sampleResult.Points);
            if (_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                if (!points.Any(p => p.X == gateX && p.Z == gateZ))
                {
                    points.Add(new SampledPoint(gateX, gateZ, 100));
                }
            }

            List<long> skipZoneTypeIds = new List<long>();
            long newZoneTypeId = ZoneTypes.Road;

            _mapGenService.AddPathsBetweenPoints(map, points, edgeSize, skipZoneTypeIds, newZoneTypeId, rand);


            SampledPoint centerPoint = new SampledPoint(map.Width / 2, map.Height / 2, sampleResult.Points.Max(x => x.Index) + 1);

            List<SampledPoint> orderedFromCenter = sampleResult.Points.OrderBy(x => x.DistanceFromCenter).ToList();

            int connectCount = RandUtils.IntRange(2, 3, rand);

            for (int i = 0; i < connectCount; i++)
            {
                SampledPoint otherPoint = orderedFromCenter[i];


                ConnectPointData point1 = new ConnectPointData() { X = centerPoint.X, Z = centerPoint.Z };

                ConnectedPairData pairData = new ConnectedPairData()
                {
                    Point1 = point1,
                    Point2 = new ConnectPointData() { X = otherPoint.X, Z = otherPoint.Z },

                };

                _mapGenService.ConnectPairOfPoints(map, pairData, edgeSize, skipZoneTypeIds, newZoneTypeId, rand);

            }



            for (int x = map.Width / 2 - 1; x <= map.Width / 2 + 1; x++)
            {
                for (int z = map.Height / 2 - 1; z <= map.Height / 2 + 1; z++)
                {
                    map.Set(x, z, CellIndex.Terrain, ZoneTypes.Road);
                }
            }

            for (int x = mapEdgeDistance; x < map.Width - 1; x++)
            {
                map.AddBits(x, 0, CellIndex.Walls, (WallTypes.Wall << MapWallBits.NWallStart));
                map.AddBits(x, map.Height - 1 - mapEdgeDistance, CellIndex.Walls, (WallTypes.Wall << MapWallBits.NWallStart));
            }

            for (int z = mapEdgeDistance; z < map.Height - 1; z++)
            {
                map.AddBits(0, z, CellIndex.Walls, (WallTypes.Wall << MapWallBits.EWallStart));
                map.AddBits(map.Width - 1 - mapEdgeDistance, z, CellIndex.Walls, (WallTypes.Wall << MapWallBits.EWallStart));
            }

            int towersPerSide = 4;

            for (int xp = 0; xp <= towersPerSide; xp++)
            {
                for (int zp = 0; zp <= towersPerSide; zp++)
                {
                    if (xp != 0 && xp != towersPerSide && zp != 0 && zp != towersPerSide)
                    {
                        continue;
                    }

                    int xx = xp * (map.Width - 1) / towersPerSide;
                    int zz = zp * (map.Height - 1) / towersPerSide;

                    map.SetEntity(xx, zz, EntityTypes.Building, BuildingTypes.GuardTower);
                }
            }

            int removeTowerX = -1;
            int removeTowerZ = -1;
            if (_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                int visGateX = 0;
                int visGateZ = 0;
                int gateBits = 0;
                bool gateIsOnSides = true;
                if (gateX == mapEdgeDistance)
                {
                    visGateX = gateX - 1;
                    visGateZ = gateZ;
                    removeTowerX = 0;
                    removeTowerZ = gateZ;
                    gateBits = WallTypes.Door << MapWallBits.EWallStart;
                }
                else if (gateX == map.Width - 1 - mapEdgeDistance)
                {
                    visGateX = gateX;
                    visGateZ = gateZ;
                    removeTowerX = map.Width - 1;
                    removeTowerZ = gateZ;
                    gateBits = WallTypes.Door << MapWallBits.EWallStart;
                }
                else if (gateZ == mapEdgeDistance)
                {
                    visGateX = gateX;
                    visGateZ = gateZ - 1;
                    removeTowerX = gateX;
                    removeTowerZ = 0;
                    gateBits = WallTypes.Door << MapWallBits.NWallStart;
                    gateIsOnSides = false;
                }
                else
                {
                    visGateX = gateX;
                    visGateZ = gateZ;
                    removeTowerX = gateX;
                    removeTowerZ = map.Height - 1;
                    gateBits = WallTypes.Door << MapWallBits.NWallStart;
                    gateIsOnSides = false;
                }


                map.Set(visGateX, visGateZ, CellIndex.Walls, gateBits);

                if (gateIsOnSides)
                {
                    map.SetEntity(removeTowerX, removeTowerZ - 1, EntityTypes.Building, BuildingTypes.GuardTower);
                    map.SetEntity(removeTowerX, removeTowerZ + 1, EntityTypes.Building, BuildingTypes.GuardTower);
                    map.SetEntity(removeTowerX, removeTowerZ, EntityTypes.Building, 0);
                }
                else
                {
                    map.SetEntity(removeTowerX - 1, removeTowerZ, EntityTypes.Building, BuildingTypes.GuardTower);
                    map.SetEntity(removeTowerX + 1, removeTowerZ, EntityTypes.Building, BuildingTypes.GuardTower);
                    map.SetEntity(removeTowerX, removeTowerZ, EntityTypes.Building, 0);
                }
            }

            IReadOnlyList<BuildingType> buildings = _gameData.Get<BuildingSettings>(null).GetData();

            List<BuildingType> crawlerBuildings = buildings.Where(x => x.IsCrawlerBuilding).ToList();

            List<BuildingType> fillerBuildings = crawlerBuildings.Where(x => x.IdKey == BuildingTypes.House).ToList();

            List<BuildingType> requiredBuildings = crawlerBuildings.Where(x => x.IdKey != BuildingTypes.House).ToList();

            List<Point2I> fillerBuildingPositions = new List<Point2I>();

            float buildingChance = RandUtils.FloatRange(0.5f,1.0f, rand);

            if (fillerBuildings.Count > 0)
            {
                for (int xx = 0; xx < map.Width; xx++)
                {

                    if (xx < mapEdgeDistance || xx >= map.Width - 1 - mapEdgeDistance)
                    {
                        continue;
                    }
                    for (int zz = 0; zz < map.Height; zz++)
                    {

                        if (zz < mapEdgeDistance || zz >= map.Height - 1 - mapEdgeDistance)
                        {
                            continue;
                        }

                        if (map.Get(xx, zz, CellIndex.Terrain) == ZoneTypes.Road)
                        {
                            continue;
                        }

                        List<Point2I> okDirs = new List<Point2I>();

                        for (int x = -1; x <= 1; x++)
                        {
                            int sx = xx + x;

                            if (sx < mapEdgeDistance || sx >= map.Width - mapEdgeDistance)
                            {
                                continue;
                            }

                            for (int z = -1; z <= 1; z++)
                            {
                                if ((x != 0) == (z != 0))
                                {
                                    continue;
                                }

                                int sz = zz + z;

                                if (sz < mapEdgeDistance || sz >= map.Height - mapEdgeDistance)
                                {
                                    continue;
                                }

                                if (map.Get(sx, sz, CellIndex.Terrain) == ZoneTypes.Road)
                                {
                                    okDirs.Add(new Point2I(x, z));
                                }
                            }
                        }

                        if (okDirs.Count > 0 && rand.NextDouble() < buildingChance)
                        {
                            Point2I okDir = okDirs[rand.Next() % okDirs.Count];

                            int dirAngle = DirUtils.DirDeltaToAngle(okDir.X, okDir.Z);

                            map.Set(xx, zz, CellIndex.Dir, dirAngle / CrawlerMapConstants.DirToAngleMult);

                            BuildingType btype = fillerBuildings[rand.Next() % fillerBuildings.Count];

                            map.SetEntity(xx, zz, EntityTypes.Building, btype.IdKey);

                            fillerBuildingPositions.Add(new Point2I(xx, zz));
                        }
                    }
                }
            }

            IReadOnlyList<ZoneType> zoneTypes = _gameData.Get<ZoneTypeSettings>(null).GetData();

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {

                    if (map.GetEntityId(x, z, EntityTypes.Building) != 0 || x == 0 || z == 0 || x == map.Width - 1 || z == map.Height - 1)
                    {
                        map.Set(x, z, CellIndex.Terrain, cityZoneTypeId);
                    }
                }
            }

            fillerBuildingPositions = fillerBuildingPositions.Where(x => x.X > mapEdgeDistance && x.Z > mapEdgeDistance &&
            x.X < map.Width - 1 - mapEdgeDistance && x.Z < map.Height - 1 - mapEdgeDistance).ToList();
            foreach (BuildingType btype in requiredBuildings)
            {
                if (fillerBuildingPositions.Count < 1)
                {
                    break;
                }

                Point2I currPoint = fillerBuildingPositions[rand.Next() % fillerBuildingPositions.Count];
                fillerBuildingPositions.Remove(currPoint);

                map.SetEntity((int)currPoint.X, (int)currPoint.Z, EntityTypes.Building, btype.IdKey);
            }

            int dungeonCount = 1;
            if (rand.NextDouble() < 0.5f)
            {
                dungeonCount++;
            }

            if (map.IdKey < 4)
            {
                dungeonCount++;
            }

            if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                dungeonCount = 1;
            }

            long dungeonLevel = map.Level;
            while (dungeonCount > 0)
            {
                if (fillerBuildingPositions.Count < 1)
                {
                    break;
                }

                dungeonCount--;

                Point2I currPoint = fillerBuildingPositions[rand.Next() % fillerBuildingPositions.Count];
                fillerBuildingPositions.Remove(currPoint);

                CrawlerMapGenData dungeonGenData = new CrawlerMapGenData()
                {
                    World = genData.World,
                    MapTypeId = CrawlerMapTypes.Dungeon,
                    Level = dungeonLevel++,
                    FromMapId = map.IdKey,
                    FromMapX = (int)(currPoint.X),
                    FromMapZ = (int)(currPoint.Z),
                };

                CrawlerMap dungeonMap = await _mapGenService.Generate(party, world, dungeonGenData, token);

                map.SetEntity((int)currPoint.X, (int)currPoint.Z, EntityTypes.Building, dungeonMap.GetBuildingTypeId());

                if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld))
                {
                    map.Details.Add(new MapCellDetail() { EntityTypeId = EntityTypes.Map, EntityId = dungeonMap.IdKey, X = currPoint.X, Z = currPoint.Z, ToX = -1, ToZ = -1 });
                }
            }

            List<long> terrains = new List<long>() { roadZoneTypeId, fillerZoneTypeId };

            for (int terrainIndex = 0; terrainIndex < 2; terrainIndex++)
            {
                for (int x = 1; x < map.Width - 1; x++)
                {
                    for (int z = 1; z < map.Height - 1; z++)
                    {
                        if (map.Get(x, z, CellIndex.Terrain) == terrains[terrainIndex])
                        {
                            if (map.Get(x, z + 1, CellIndex.Terrain) == terrains[(terrainIndex + 1) % terrains.Count])
                            {
                                map.AddBits(x, z, CellIndex.Walls, WallTypes.Barricade << MapWallBits.NWallStart);
                            }
                            if (map.Get(x + 1, z, CellIndex.Terrain) == terrains[(terrainIndex + 1) % terrains.Count])
                            {
                                map.AddBits(x, z, CellIndex.Walls, WallTypes.Barricade << MapWallBits.EWallStart);
                            }
                        }
                    }
                }
            }


            if (removeTowerX >= 0 && removeTowerZ >= 0)
            {
                map.Set(removeTowerX, removeTowerZ, CellIndex.Terrain, ZoneTypes.Road);
            }

            await AddMapNpcs(party, world, genData, map, fillerBuildingPositions, rand);

            return new NewCrawlerMap() { Map = map, EnterX = gateX, EnterZ = gateZ};
        }

        public override NpcQuestMaps GetQuestMapsForNpc(PartyData party, CrawlerWorld world, CrawlerMap map, MapCellDetail npcDetail, IRandom rand)
        {

            NpcQuestMaps maps = new NpcQuestMaps();

            CrawlerQuestSettings questSettings = _gameData.Get<CrawlerQuestSettings>(_gs.ch);

            List<MapCellDetail> exitDetails = map.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

            foreach (MapCellDetail cityExitDetails in exitDetails)
            {
                CrawlerMap otherMap = world.GetMap(cityExitDetails.EntityId);

                if (otherMap != null)
                {
                    if (otherMap.CrawlerMapTypeId == CrawlerMapTypes.Dungeon)
                    {
                        maps.PrimaryMaps.Add(new MapLink() { Map = otherMap, Link = cityExitDetails });
                    }
                }
                else
                {
                    CrawlerMap outdoorMap = world.GetMap(cityExitDetails.EntityId);

                    if (outdoorMap != null && outdoorMap.CrawlerMapTypeId == CrawlerMapTypes.Outdoors)
                    {
                        List<MapCellDetail> startNearbyEntrances = map.Details.Where(e => e.EntityTypeId == EntityTypes.Map &&
                        MathUtil.PythagoreanDistance(cityExitDetails.ToX - e.X, cityExitDetails.Z - e.Z) <
                            questSettings.MaxDistanceFromQuestGiverToTargetMap).ToList();

                        foreach (MapCellDetail nearbyDetail in startNearbyEntrances)
                        {
                            CrawlerMap nearbyMap = world.GetMap(nearbyDetail.EntityId);

                            if (nearbyMap != null && nearbyMap.CrawlerMapTypeId == CrawlerMapTypes.Dungeon)
                            {
                                maps.SecondaryMaps.Add(new MapLink() { Map = nearbyMap, Link = nearbyDetail });
                            }
                        }
                    }
                }
            }

            return maps;
        }
    }
}



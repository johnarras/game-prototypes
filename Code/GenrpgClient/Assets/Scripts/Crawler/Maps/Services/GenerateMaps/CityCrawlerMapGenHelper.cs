
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Zones.Settings;
using UnityEngine;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Zones.Constants;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.MapGen.Entities;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Chests.Settings;
using System.Threading;
using Genrpg.Shared.Crawler.Quests.Settings;

namespace Assets.Scripts.Crawler.Maps.Services.GenerateMaps
{
    public class CityCrawlerMapGenHelper : BaseCrawlerMapGenHelper
    {

        public override long Key => CrawlerMapTypes.City;

        public override async Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData, CancellationToken token)
        {
            await Task.CompletedTask;
            MyRandom rand = new MyRandom(genData.World.Seed / 2 + genData.World.MaxMapId * 17);

            IReadOnlyList<ZoneType> allZoneTypes = _gameData.Get<ZoneTypeSettings>(null).GetData();

            long cityZoneTypeId = allZoneTypes.FirstOrDefault(x => x.Name == "City")?.IdKey ?? 1;
            long roadZoneTypeId = allZoneTypes.FirstOrDefault(x => x.Name == "Road")?.IdKey ?? 1;
            long fillerZoneTypeId = ZoneTypes.Field;
            CrawlerMapGenType genType = genData.GenType;

            int mapEdgeDistance = 1;

            int width = (int)MathUtils.LongRange(genType.MinWidth, genType.MaxWidth, rand);
            int height = (int)MathUtils.LongRange(genType.MinHeight, genType.MaxHeight, rand);
            CrawlerMap map = _worldService.CreateMap(genData, width, height);

            map.Name = _zoneGenService.GenerateZoneName(genData.ZoneType.IdKey, rand.Next(), true);
            bool[,] clearCells = AddCorridors(map, genData, rand, MathUtils.FloatRange(genType.MinCorridorDensity, genType.MaxCorridorDensity, rand));

            int gateX = -1;
            int gateZ = -1;

            int cityEdgeDistance = 1;
            // X on border, y in middle.
            if (rand.NextDouble() < 0.5f)
            {
                gateX = (rand.NextDouble() < 0.5 ? cityEdgeDistance : map.Width - 1 - cityEdgeDistance);
                gateZ = MathUtils.IntRange(map.Height / 3, map.Height * 2 / 3, rand);

                int start = gateX;
                int dir = (gateX == cityEdgeDistance ? 1 : -1);
                int xx = start;
                while (true)
                {
                    if (clearCells[xx, gateZ])
                    {
                        break;
                    }
                    clearCells[xx, gateZ] = true;
                    xx += dir;
                    if (xx < 0 || xx >= map.Width)
                    {
                        break;
                    }
                }
            }
            else
            {
                gateZ = (rand.NextDouble() < 0.5 ? cityEdgeDistance : map.Height - 1 - cityEdgeDistance);
                gateX = MathUtils.IntRange(map.Width / 3, map.Width * 2 / 3, rand);

                int start = gateZ;
                int dir = (gateZ == cityEdgeDistance ? 1 : -1);

                int zz = start;
                while (true)
                {
                    if (clearCells[gateX, zz])
                    {
                        break;
                    }
                    clearCells[gateX, zz] = true;
                    zz += dir;
                    if (zz < 0 || zz >= map.Height)
                    {
                        break;
                    }
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

            int visGateX = 0;
            int visGateZ = 0;
            int gateBits = 0;
            int towerX = 0;
            int towerZ = 0;
            bool gateIsOnSides = true;
            if (gateX == mapEdgeDistance)
            {
                visGateX = gateX - 1;
                visGateZ = gateZ;
                towerX = 0;
                towerZ = gateZ;
                gateBits = WallTypes.Door << MapWallBits.EWallStart;
            }
            else if (gateX == map.Width - 1 - mapEdgeDistance)
            {
                visGateX = gateX;
                visGateZ = gateZ;
                towerX = map.Width - 1;
                towerZ = gateZ;
                gateBits = WallTypes.Door << MapWallBits.EWallStart;
            }
            else if (gateZ == mapEdgeDistance)
            {
                visGateX = gateX;
                visGateZ = gateZ - 1;
                towerX = gateX;
                towerZ = 0;
                gateBits = WallTypes.Door << MapWallBits.NWallStart;
                gateIsOnSides = false;
            }
            else
            {
                visGateX = gateX;
                visGateZ = gateZ;
                towerX = gateX;
                towerZ = map.Height - 1;
                gateBits = WallTypes.Door << MapWallBits.NWallStart;
                gateIsOnSides = false;
            }


            map.Set(visGateX, visGateZ, CellIndex.Walls, gateBits);

            if (gateIsOnSides)
            {
                map.SetEntity(towerX, towerZ - 1, EntityTypes.Building, BuildingTypes.GuardTower);
                map.SetEntity(towerX, towerZ + 1, EntityTypes.Building, BuildingTypes.GuardTower);
                map.SetEntity(towerX, towerZ, EntityTypes.Building, 0);
            }
            else
            {
                map.SetEntity(towerX - 1, towerZ, EntityTypes.Building, BuildingTypes.GuardTower);
                map.SetEntity(towerX + 1, towerZ, EntityTypes.Building, BuildingTypes.GuardTower);
                map.SetEntity(towerX, towerZ, EntityTypes.Building, 0);
            }

            IReadOnlyList<BuildingType> buildings = _gameData.Get<BuildingSettings>(null).GetData();

            List<BuildingType> crawlerBuildings = buildings.Where(x => x.IsCrawlerBuilding).ToList();

            List<BuildingType> fillerBuildings = crawlerBuildings.Where(x => x.VariationCount > 1).ToList();

            List<BuildingType> requiredBuildings = crawlerBuildings.Where(x => x.VariationCount == 1).ToList();

            List<PointXZ> fillerBuildingPositions = new List<PointXZ>();

            float buildingChance = MathUtils.FloatRange(genType.MinBuildingDensity, genType.MaxBuildingDensity, rand);

            if (fillerBuildings.Count > 0)
            {
                for (int xx = 0; xx < clearCells.GetLength(0); xx++)
                {

                    if (xx < mapEdgeDistance || xx >= map.Width - 1 - mapEdgeDistance)
                    {
                        continue;
                    }
                    for (int zz = 0; zz < clearCells.GetLength(1); zz++)
                    {

                        if (zz < mapEdgeDistance || zz >= map.Height - 1 - mapEdgeDistance)
                        {
                            continue;
                        }

                        if (clearCells[xx, zz])
                        {
                            continue;
                        }

                        List<MyPoint> okDirs = new List<MyPoint>();

                        for (int x = -1; x <= 1; x++)
                        {
                            int sx = xx + x;

                            if (sx < mapEdgeDistance || sx >= map.Width - mapEdgeDistance)
                            {
                                continue;
                            }

                            for (int y = -1; y <= 1; y++)
                            {
                                if ((x != 0) == (y != 0))
                                {
                                    continue;
                                }

                                int sy = zz + y;

                                if (sy < mapEdgeDistance || sy >= map.Height - mapEdgeDistance)
                                {
                                    continue;
                                }

                                if (clearCells[sx, sy] == true)
                                {
                                    okDirs.Add(new MyPoint(x, y));
                                }
                            }
                        }

                        if (okDirs.Count > 0 && rand.NextDouble() < buildingChance)
                        {
                            MyPoint okDir = okDirs[rand.Next() % okDirs.Count];

                            int dirAngle = DirUtils.DirDeltaToAngle(okDir.X, okDir.Y);

                            map.Set(xx, zz, CellIndex.Dir, dirAngle / CrawlerMapConstants.DirToAngleMult);

                            BuildingType btype = fillerBuildings[rand.Next() % fillerBuildings.Count];

                            map.SetEntity(xx, zz, EntityTypes.Building, btype.IdKey);

                            fillerBuildingPositions.Add(new PointXZ(xx, zz));
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
                    else if (clearCells[x, z])
                    {
                        map.Set(x, z, CellIndex.Terrain, roadZoneTypeId);
                    }
                    else
                    {

                        map.Set(x, z, CellIndex.Terrain, fillerZoneTypeId);
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

                PointXZ currPoint = fillerBuildingPositions[rand.Next() % fillerBuildingPositions.Count];
                fillerBuildingPositions.Remove(currPoint);

                map.SetEntity((int)currPoint.X, (int)currPoint.Z, EntityTypes.Building, btype.IdKey);
            }

            int dungeonCount = 1;
            if (rand.NextDouble() < 0.5f)
            {
                dungeonCount++;
            }
            if (rand.NextDouble() < 0.25f)
            {
                dungeonCount++;
            }

            if (map.IdKey < 4)
            {
                dungeonCount++;
            }
            int dungeonLevel = map.Level;
            while (dungeonCount > 0)
            {
                if (fillerBuildingPositions.Count < 1)
                {
                    break;
                }

                dungeonCount--;

                PointXZ currPoint = fillerBuildingPositions[rand.Next() % fillerBuildingPositions.Count];
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

                map.SetEntity((int)currPoint.X, (int)currPoint.Z, EntityTypes.Building, dungeonMap.BuildingTypeId);

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

            await AddMapNpcs(party, world, genData, map, fillerBuildingPositions, rand);

            return new NewCrawlerMap() { Map = map, EnterX = gateX, EnterZ = gateZ };   
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
                        MathUtils.PythagoreanDistance( cityExitDetails.ToX - e.X, cityExitDetails.Z - e.Z) <
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

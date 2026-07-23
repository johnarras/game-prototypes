using OxDb.Client.Crawler.Maps.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Buildings.Constants;
using OxDb.SharedGame.Crawler.Loot.Services;
using OxDb.SharedGame.Crawler.MapGen.Entities;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Maps.Settings;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Quests.Settings;
using OxDb.SharedGame.Crawler.Upgrades.Constants;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.ProcGen.Entities;
using OxDb.SharedGame.Riddles.Services;
using OxDb.SharedGame.Riddles.Settings;
using OxDb.SharedGame.Zones.Constants;
using OxDb.SharedGame.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.MapGen.Helpers
{
    public class OutdoorCrawlerMapGenHelper : BaseCrawlerMapGenHelper
    {
        private ILootGenService _lootGenService = null;
        private IRiddleService _riddleService = null;

        public override long HelperKey => CrawlerMapTypes.Outdoors;

        public override async Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData, CancellationToken token)
        {
            IRandom rand = new MyRandom(genData.World.Seed / 4 + genData.World.GetMaxMapId() * 131);
            int width = RandUtils.IntRange(genData.GenType.MinWidth, genData.GenType.MaxWidth, rand);
            int height = RandUtils.IntRange(genData.GenType.MinHeight, genData.GenType.MaxHeight, rand);

            CrawlerMap outdoorMap = _worldService.CreateMap(genData, width, height);
            outdoorMap.ZoneUnits = new List<ZoneUnitSpawn>();

            byte[,] overrides = new byte[outdoorMap.Width, outdoorMap.Height];
            long[,] zoneTypeIds = new long[outdoorMap.Width, outdoorMap.Height];
            long[,] regionIds = new long[outdoorMap.Width, outdoorMap.Height];
            Reward[,] objects = new Reward[outdoorMap.Width, outdoorMap.Height];

            List<ZoneRegion> regions = new List<ZoneRegion>();

            ZoneTypeSettings zoneSettings = _gameData.Get<ZoneTypeSettings>(_gs.ch);

            List<ZoneType> allZoneTypes = zoneSettings.GetData().OrderBy(x => x.MinLevel).ToList();

            List<long> okZoneTypeIds = allZoneTypes.Where(x => x.IsOutdoors).Select(x => x.IdKey).ToList();

            int startMapEdgeSize = 4;

            int cityDistanceFromEdge = startMapEdgeSize * 2;

            int genWidth = outdoorMap.Width - cityDistanceFromEdge * 2;
            int genHeight = outdoorMap.Height - cityDistanceFromEdge * 2;

            int cellCount = genWidth * genHeight;


            double density = RandUtils.IntRange(100, 300, rand);

            int regionCount = (int)(cellCount / density);

            int minSeparation = (int)Math.Sqrt(density);

            SamplingData samplingData = new SamplingData()
            {
                Count = regionCount,
                MaxAttemptsPerItem = 20,
                MinX = cityDistanceFromEdge,
                MaxX = outdoorMap.Width - cityDistanceFromEdge,
                MinZ = cityDistanceFromEdge,
                MaxZ = outdoorMap.Height - cityDistanceFromEdge,
                MinSeparation = minSeparation,
                Seed = rand.Next(),
                CreateIndexGrid = true,
            };

            SamplingResult result = _samplingService.PlanePoissonSample(samplingData);

            List<SampledPoint> points = result.Points;

            int sortx = (rand.NextDouble() < 0.5 ? -1 : 1);
            int sortz = (rand.NextDouble() < 0.5 ? -1 : 1);

            points = points.OrderBy(p => p.X * sortx).ThenBy(p => p.Z * sortz).ToList();

            SampledPoint firstPoint = points[0];

            List<SampledPoint> orderedPoints = points.OrderBy(p =>
                Math.Sqrt(
                    (p.X - firstPoint.X) * (p.X - firstPoint.X) +
                    (p.Z - firstPoint.Z) * (p.Z - firstPoint.Z)
                    )).ToList();

            float addChance = 0.66f;

            List<SampledPoint> origPoints = new List<SampledPoint>();
            origPoints.Add(firstPoint);

            orderedPoints.Remove(firstPoint);

            List<SampledPoint> waterPoints = new List<SampledPoint>();

            foreach (SampledPoint sampled in orderedPoints)
            {
                if (rand.NextDouble() < addChance)
                {
                    origPoints.Add(sampled);
                }
                else
                {
                    waterPoints.Add(sampled);
                }
            }


            long maxLevel = _gameData.Get<CrawlerMapSettings>(_gs.ch).MaxLevel;


            List<ZoneType> allOkZones = allZoneTypes.Where(x => x.GenChance > 0 && x.IdKey != ZoneTypes.Mountains).ToList();

            int maxRegionId = 0;
            List<ZoneType> currentOkZones = new List<ZoneType>(allOkZones);


            float startRegionX = points[0].X;
            float startRegionZ = points[0].Z;

            points = points.OrderBy(p => Mathf.Sqrt((p.X - startRegionX) * (p.X - startRegionX) + (p.Z - startRegionZ) * (p.Z - startRegionZ))).ToList();

            while (points.Count > 0)
            {
                if (currentOkZones.Count < 1)
                {
                    currentOkZones = new List<ZoneType>(allOkZones);
                }

                SampledPoint centerPoint = points[0];

                points.Remove(centerPoint);

                ZoneType biomeType = currentOkZones[rand.Next() % currentOkZones.Count];

                bool isWaterRegion = false;
                if (waterPoints.Contains(centerPoint))
                {
                    biomeType = zoneSettings.Get(ZoneTypes.Water);
                    isWaterRegion = true;
                }

                currentOkZones.Remove(biomeType);

                ZoneRegion region = new ZoneRegion()
                {
                    CenterX = (int)centerPoint.X,
                    CenterZ = (int)centerPoint.Z,
                    ZoneTypeId = biomeType.IdKey,
                    RegionId = ++maxRegionId,
                    IsWaterRegion = isWaterRegion,
                };

                regions.Add(region);
            }

            double levelDelta = 1.0f * maxLevel / regions.Count;

            if (levelDelta > 2)
            {
                levelDelta = 2;
            }

            for (int r = 0; r < regions.Count; r++)
            {
                regions[r].Level = 1 + (int)(r * levelDelta);
            }

            long cityZoneTypeId = allZoneTypes.FirstOrDefault(x => x.Name == "City").IdKey;
            long waterZoneTypeId = allZoneTypes.FirstOrDefault(x => x.Name == "Water").IdKey;
            long waterRegionId = ++maxRegionId;
            long roadZoneTypeId = allZoneTypes.FirstOrDefault(x => x.Name == "Road").IdKey;
            long roadRegionId = ++maxRegionId;
            long mountainZoneTypeId = allZoneTypes.FirstOrDefault(x => x.Name == "Mountains").IdKey;
            long mountainRegionId = ++maxRegionId;


            if (regions.Count < 1)
            {
                return new NewCrawlerMap() { Map = outdoorMap };
            }

            float radiusDelta = 0.2f;

            int radius = 0;
            while (true)
            {
                bool foundUnsetCell = false;
                for (int x = 0; x < outdoorMap.Width; x++)
                {
                    for (int z = 0; z < outdoorMap.Height; z++)
                    {
                        if (zoneTypeIds[x, z] == 0)
                        {
                            foundUnsetCell = true;
                            break;
                        }
                    }
                    if (foundUnsetCell)
                    {
                        break;
                    }
                }

                if (!foundUnsetCell)
                {
                    break;
                }

                radius++;

                outdoorMap.Regions = regions;

                float spreadDelta = 0.4f;
                float dirDelta = 0.3f;
                foreach (ZoneRegion region in regions)
                {
                    region.Name = _zoneGenService.GenerateZoneName(region.ZoneTypeId, rand.Next(), false);
                    float currRadius = RandUtils.FloatRange(radius * (1 - radiusDelta), radius * (1 + radiusDelta), rand);

                    float xrad = currRadius * RandUtils.DeltaScale(spreadDelta, rand);
                    float zrad = currRadius * RandUtils.DeltaScale(spreadDelta, rand);
                    float xcenter = region.CenterX + RandUtils.DeltaScale(dirDelta,rand) * currRadius;
                    float zcenter = region.CenterZ * RandUtils.DeltaScale(dirDelta, rand) * currRadius;

                    xcenter = region.CenterX;
                    zcenter = region.CenterZ;

                    int xmin = MathUtil.Clamp(0, (int)(xcenter - xrad - 1), outdoorMap.Width - 1);
                    int xmax = MathUtil.Clamp(0, (int)(xcenter + xrad + 1), outdoorMap.Width - 1);

                    int zmin = MathUtil.Clamp(0, (int)(zcenter - zrad - 1), outdoorMap.Height - 1);
                    int zmax = MathUtil.Clamp(0, (int)(zcenter + zrad + 1), outdoorMap.Height - 1);

                    for (int x = xmin; x <= xmax; x++)
                    {
                        for (int z = zmin; z <= zmax; z++)
                        {

                            if (zoneTypeIds[x, z] != 0)
                            {
                                continue;
                            }

                            float xpct = (x - xcenter) / xrad;
                            float zpct = (z - zcenter) / zrad;

                            float distScale = Mathf.Sqrt(xpct * xpct + zpct * zpct);

                            if (distScale <= 1)
                            {
                                zoneTypeIds[x, z] = region.ZoneTypeId;
                                regionIds[x, z] = region.RegionId;
                            }
                        }
                    }
                }
            }



            List<float> cornerRadii = new List<float>();

            float minCornerRadius = 12;
            float maxCornerRadius = 20;

            for (int c = 0; c < 4; c++)
            {
                cornerRadii.Add(RandUtils.FloatRange(minCornerRadius, maxCornerRadius, rand));
            }

            int maxCheckRadius = (int)(maxCornerRadius + startMapEdgeSize);

            int xcorner = 0;
            int zcorner = 0;
            for (int x = 0; x < outdoorMap.Width; x++)
            {

                for (int z = 0; z < outdoorMap.Height; z++)
                {
                    int cornerIndex = -1;

                    if (x <= maxCheckRadius)
                    {
                        xcorner = 0;
                        if (z <= maxCheckRadius)
                        {
                            zcorner = 0;
                            cornerIndex = 0;
                        }
                        else if (z >= outdoorMap.Height - maxCheckRadius - 1)
                        {
                            cornerIndex = 1;
                            zcorner = outdoorMap.Height - 1;
                        }
                    }
                    else if (x >= outdoorMap.Width - maxCheckRadius - 1)
                    {
                        xcorner = outdoorMap.Width - 1;
                        if (z <= maxCheckRadius)
                        {
                            cornerIndex = 2;
                            zcorner = 0;
                        }
                        else if (z >= outdoorMap.Height - maxCheckRadius - 1)
                        {
                            cornerIndex = 3;
                            zcorner = outdoorMap.Height - 1;
                        }
                    }

                    int mapEdgeSize = startMapEdgeSize + RandUtils.IntRange(-1, 1, rand);
                    if ((x < mapEdgeSize || x >= outdoorMap.Width - mapEdgeSize) ||
                        (z < mapEdgeSize || z >= outdoorMap.Height - mapEdgeSize))
                    {
                        zoneTypeIds[x, z] = waterZoneTypeId;
                    }


                    if (cornerIndex >= 0 && cornerIndex < cornerRadii.Count)
                    {
                        int currRadius = (int)cornerRadii[cornerIndex] + startMapEdgeSize;


                        int cx = xcorner;
                        int cz = zcorner;

                        if (cx > 0)
                        {
                            cx -= currRadius;
                        }
                        else
                        {
                            cx += currRadius;
                        }

                        if (cz > 0)
                        {
                            cz -= currRadius;

                        }
                        else
                        {
                            cz += currRadius;
                        }

                        if (cx < outdoorMap.Width / 2 && x > cx)
                        {
                            continue;
                        }

                        if (cx > outdoorMap.Width / 2 && x < cx)
                        {
                            continue;
                        }

                        if (cz < outdoorMap.Height / 2 && z > cz)
                        {
                            continue;
                        }

                        if (cz > outdoorMap.Height / 2 && z < cz)
                        {
                            continue;
                        }

                        float currDist = Mathf.Sqrt((x - cx) * (x - cx) + (z - cz) * (z - cz));

                        currDist += RandUtils.DeltaRange(1, rand);

                        if (currDist >= currRadius && zoneTypeIds[x, z] != waterZoneTypeId)
                        {
                            zoneTypeIds[x, z] = waterZoneTypeId;
                        }
                    }
                }
            }




            // Roads between cities

            List<long> skipZoneTypeIds = new List<long>() { ZoneTypes.Water };
            long newZoneTypeId = ZoneTypes.Road;
            _mapGenService.AddPathsBetweenPoints(outdoorMap, origPoints, 6, skipZoneTypeIds, newZoneTypeId, rand);



            // Mountains at zone borders. (okZoneIds if  two diff make a small blob...only replacing things in ok biomeIds

            int crad = 1;
            int rrad = 2;
            int trad = Math.Max(crad, rrad);
            for (int x = trad; x < outdoorMap.Width - trad; x++)
            {
                for (int z = trad; z < outdoorMap.Height - trad; z++)
                {
                    List<long> currOkZoneTypeIds = new List<long>();
                    bool nearRoad = false;

                    // Check for roads.
                    for (int xx = x - rrad; xx <= x + rrad; xx++)
                    {
                        for (int zz = z - rrad; zz <= z + rrad; zz++)
                        {
                            if (zoneTypeIds[xx, zz] == roadZoneTypeId)
                            {
                                nearRoad = true;
                                break;
                            }
                        }
                    }

                    if (nearRoad)
                    {
                        continue;
                    }

                    // Now check smaller radius for diff biomes.
                    for (int xx = x - crad; xx <= x + crad; xx++)
                    {
                        for (int zz = z - crad; zz <= z + crad; zz++)
                        {
                            long ztid = zoneTypeIds[xx, zz];
                            if (ztid != mountainZoneTypeId && okZoneTypeIds.Contains(ztid))
                            {
                                if (!currOkZoneTypeIds.Contains(ztid))
                                {
                                    currOkZoneTypeIds.Add(ztid);
                                }
                            }
                        }
                    }

                    int nrad = rand.NextDouble() < 0.05f ? 1 : 0;

                    if (currOkZoneTypeIds.Count > 1)
                    {
                        for (int xx = x - nrad; xx <= x + nrad; xx++)
                        {
                            for (int zz = z - nrad; zz <= z + nrad; zz++)
                            {
                                zoneTypeIds[xx, zz] = mountainZoneTypeId;
                            }
                        }
                    }
                }
            }


            double randomTerrainChance = 0.02f;

            // Randomize cells a bit
            for (int x = 0; x < outdoorMap.Width; x++)
            {
                for (int z = 0; z < outdoorMap.Height; z++)
                {
                    if (okZoneTypeIds.Contains(zoneTypeIds[x, z]) && rand.NextDouble() < randomTerrainChance)
                    {
                        zoneTypeIds[x, z] = okZoneTypeIds[rand.Next() % okZoneTypeIds.Count];
                    }
                }
            }


            ZoneType cityZoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(ZoneTypes.City);

            for (int c = 0; c < origPoints.Count; c++)
            {
                Point2I pt = origPoints[c];

                int cityLevel = 1;
                ZoneRegion zoneRegion = regions.FirstOrDefault(x => x.CenterX == (int)pt.X && x.CenterZ == (int)pt.Z);

                if (zoneRegion != null)
                {
                    cityLevel = (int)zoneRegion.Level;
                }

                zoneTypeIds[(int)pt.X, (int)pt.Z] = cityZoneTypeId;
                outdoorMap.Set((int)pt.X, (int)pt.Z, CellIndex.Terrain, cityZoneTypeId);
                CrawlerMapGenData cityGenData = new CrawlerMapGenData()
                {
                    World = genData.World,
                    MapTypeId = CrawlerMapTypes.City,
                    Level = cityLevel,
                    FromMapId = outdoorMap.IdKey,
                    FromMapX = (int)(pt.X),
                    FromMapZ = (int)(pt.Z),
                    ZoneType = cityZoneType,
                };

                outdoorMap.SetEntity((int)(pt.X), (int)pt.Z, EntityTypes.Building, BuildingTypes.City);

                int xx = (int)pt.X;
                int zz = (int)pt.Z;

                int dx = 0;
                int dz = 0;

                if (outdoorMap.Get(xx, zz + 1, CellIndex.Terrain) == ZoneTypes.Road)
                {
                    dx = 0;
                    dz = 1;
                }
                else if (outdoorMap.Get(xx, zz - 1, CellIndex.Terrain) == ZoneTypes.Road)
                {
                    dx = 0;
                    dz = -1;
                }
                else if (outdoorMap.Get(xx - 1, zz, CellIndex.Terrain) == ZoneTypes.Road)
                {
                    dx = -1;
                    dz = 0;
                }
                else if (outdoorMap.Get(xx + 1, zz, CellIndex.Terrain) == ZoneTypes.Road)
                {
                    dx = 1;
                    dz = 0;
                }

                int dirAngle = DirUtils.DirDeltaToAngle(dx, dz);

                outdoorMap.Set(xx, zz, CellIndex.Dir, dirAngle / CrawlerMapConstants.DirToAngleMult);

                CrawlerMap cityMap = await _mapGenService.Generate(party, world, cityGenData, token);

                cityMap.FromPlaceName = outdoorMap.GetName(xx, zz);

                cityMap.Level = _worldService.GetMapLevelAtPoint(world, outdoorMap.IdKey, xx, zz);
            }

            // Add random dungeons and stuff on the map
            samplingData = new SamplingData()
            {
                Count = outdoorMap.Width * outdoorMap.Height / 500,
                MaxAttemptsPerItem = 5,
                MinX = cityDistanceFromEdge,
                MaxX = outdoorMap.Width - cityDistanceFromEdge,
                MinZ = cityDistanceFromEdge,
                MaxZ = outdoorMap.Height - cityDistanceFromEdge,
                MinSeparation = 7,
                Seed = rand.Next(),
            };

            SamplingResult dungeonResult = _samplingService.PlanePoissonSample(samplingData);

            List<Point2I> startDungeonPoints = dungeonResult.Points.Cast<Point2I>().ToList();

            List<Point2I> finalDungeonPoints = new List<Point2I>();

            double minDistFromCity = 5;

            int dungeonAttempts = startDungeonPoints.Count;
            int dungeonSuccess = 0;
            foreach (Point2I p in startDungeonPoints)
            {
                int xx = (int)p.X;
                int zz = (int)p.Z;

                if (!okZoneTypeIds.Contains(outdoorMap.Get(xx, zz, CellIndex.Terrain)))
                {
                    continue;
                }

                bool tooCloseToCity = false;
                foreach (ZoneRegion region in outdoorMap.Regions)
                {
                    double ddx = region.CenterX - xx;
                    double ddz = region.CenterZ - zz;

                    if (Math.Sqrt(ddx * ddx + ddz * ddz) < minDistFromCity)
                    {
                        tooCloseToCity = true;
                        break;
                    }
                }

                if (tooCloseToCity)
                {
                    continue;
                }

                long dungeonLevel = 2 + _worldService.GetMapLevelAtPoint(world, outdoorMap.IdKey, xx, zz) * 5 / 4;
                CrawlerMapGenData dungeonGenData = new CrawlerMapGenData()
                {
                    World = genData.World,
                    MapTypeId = CrawlerMapTypes.Dungeon,
                    Level = (int)dungeonLevel,
                    FromMapId = outdoorMap.IdKey,
                    FromMapX = xx,
                    FromMapZ = zz,
                };

                CrawlerMap dungeonMap = await _mapGenService.Generate(party, world, dungeonGenData, token);

                dungeonSuccess++;

                finalDungeonPoints.Add(new Point2I(xx, zz));

                outdoorMap.SetEntity(xx, zz, EntityTypes.Building, dungeonMap.GetBuildingTypeId());
            }

            List<Riddle> riddles = _gameData.Get<RiddleSettings>(_gs.ch).GetData().ToList();

            CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

            List<CrawlerMap> dungeonMaps = world.Maps.Where(x => x.CrawlerMapTypeId == CrawlerMapTypes.Dungeon && x.MapFloor == 1).OrderBy(x => x.Level).ToList();

            List<List<CrawlerMap>> dungeonMapGroups = world.Maps.GroupBy(x => x.BaseCrawlerMapId).Select(y => y.OrderBy(z => z.MapFloor).ToList()).ToList();

            for (int d = 0; d < dungeonMaps.Count; d++)
            {
                CrawlerMap dmap = dungeonMaps[d];

                List<CrawlerMap> otherDungeonMaps = world.Maps.Where(x => x.CrawlerMapTypeId == CrawlerMapTypes.Dungeon &&
                x.Name == dmap.Name && x.IdKey >= dmap.IdKey && x.IdKey <= dmap.IdKey + 6).OrderBy(x => x.MapFloor).ToList();

                dungeonMapGroups.Add(otherDungeonMaps);
            }

            for (int d = 0; d < dungeonMapGroups.Count; d++)
            {
                List<CrawlerMap> floors = dungeonMapGroups[d];

                List<long> floorIds = floors.Select(x => x.IdKey).ToList();

                CrawlerMap entranceMap = floors.First();

                MapCellDetail exitDetail = entranceMap.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map &&
                !floorIds.Contains(x.EntityId));

                if (exitDetail != null)
                {
                    CrawlerMap exitMap = world.GetMap(exitDetail.EntityId);
                    if (exitMap != null)
                    {
                        entranceMap.FromPlaceName = exitMap.GetName(exitDetail.ToX, exitDetail.ToZ);
                    }
                }
            }

            List<int> questItemIndexesUsed = new List<int>();

            long gameDungeonUnlockLevel = Math.Max(mapSettings.MinQuestUnlockDungeonLevel,
                party.GetUpgradePointsLevel(UpgradeReasons.CompleteDungeon, true));

            bool haveUnlockQuests = _optionsService.HasOption(party, CrawlerOptions.Puzzles);
            for (int dungeonIndex = 0; dungeonIndex < dungeonMapGroups.Count; dungeonIndex++)
            {
                List<CrawlerMap> floors = dungeonMapGroups[dungeonIndex];

                List<long> floorIds = floors.Select(x => x.IdKey).ToList();

                CrawlerMap entranceMap = floors.First();

                if (haveUnlockQuests && entranceMap.Level >= gameDungeonUnlockLevel && rand.NextDouble() < mapSettings.QuestItemEntranceUnlockChance)

                {
                    string questItemName = _lootGenService.GenerateItemNames(rand, 1, 100, "Key").First().SingularName;

                    int lookbackDistance = 6;

                    List<int> okIndexes = new List<int>();

                    for (int i = dungeonIndex - 1; i >= 0 && dungeonIndex - i <= lookbackDistance + 1; i--)
                    {
                        if (dungeonMapGroups[i].FastAny(x => x.Level >= entranceMap.Level) ||
                            !dungeonMapGroups[i].FastAny(x => x.Level >= mapSettings.MinQuestItemDungeonLevel))
                        {
                            continue;
                        }

                        if (questItemIndexesUsed.Contains(i))
                        {
                            continue;
                        }

                        okIndexes.Add(i);
                    }

                    if (okIndexes.Count < 1)
                    {
                        continue;
                    }

                    int chosenIndex = okIndexes[rand.Next() % okIndexes.Count];

                    List<CrawlerMap> questItemContainingMaps = dungeonMapGroups[chosenIndex];

                    List<MapCellDetail> openQuestDetails = new List<MapCellDetail>();

                    List<CrawlerMap> okMaps = new List<CrawlerMap>();

                    foreach (CrawlerMap cmap in questItemContainingMaps)
                    {

                        List<MapEntity> startEntities = cmap.GetMapEntities(EntityTypes.QuestItem, byte.MaxValue);

                        if (startEntities.Count > 0 && cmap.Level < entranceMap.Level &&
                            cmap.Level >= mapSettings.MinQuestItemDungeonLevel)
                        {
                            okMaps.Add(cmap);
                        }
                    }

                    if (okMaps.Count < 1)
                    {
                        continue;
                    }

                    CrawlerMap questItemMap = okMaps[rand.Next() % okMaps.Count];

                    if (questItemMap.Level >= entranceMap.Level)
                    {
                        _logService.Info("Warning: Dungeon level " + entranceMap.Level + " MapId: " + entranceMap.IdKey + " had entrance quest item in "
                            + " a map of level " + questItemMap.Level + " MapId: " + questItemMap.IdKey);
                        continue;
                    }

                    List<MapEntity> okMapEntities = questItemMap.GetMapEntities(EntityTypes.QuestItem, byte.MaxValue);

                    MapEntity chosenMapEntity = okMapEntities[rand.Next() % okMapEntities.Count];

                    if (questItemMap != null && chosenMapEntity != null)
                    {
                        questItemIndexesUsed.Add(chosenIndex);
                        long nextQuestItemId = 1;
                        if (world.QuestItems.Count > 0)
                        {
                            nextQuestItemId = CollectionUtils.GetNextIdKey(world.QuestItems, 0);
                        }

                        WorldQuestItem wqi = new WorldQuestItem()
                        {
                            IdKey = nextQuestItemId,
                            Name = questItemName,
                            FoundInMapId = questItemMap.IdKey,
                            UnlocksMapId = entranceMap.IdKey,
                        };
                        world.QuestItems.Add(wqi);

                        questItemMap.SetEntity(chosenMapEntity.X, chosenMapEntity.Z, EntityTypes.QuestItem, wqi.IdKey);

                        if (questItemMap.ZoneUnits.Count > 0)
                        {
                            questItemMap.ZoneUnits = questItemMap.ZoneUnits.OrderBy(x => HashUtils.NewGuid()).ToList();
                            questItemMap.ZoneUnits = questItemMap.ZoneUnits.OrderBy(x => x.Weight).ToList();

                            ZoneUnitSpawn firstUnit = questItemMap.ZoneUnits.First();

                            wqi.GuardUnitTypeId = firstUnit.UnitTypeId;
                            wqi.GuardName = _nameGenService.GenerateUnitName(rand, true);

                        }

                        _logService.Info("Map " + entranceMap.IdKey + " Lev: " + entranceMap.Level + " has quest item in map level " +
                            questItemMap.Level + " Dungeon group index " + chosenIndex);
                        entranceMap.MapQuestItemId = nextQuestItemId;
                    }
                }

                await _riddleService.GenerateRiddles(party, floors, genData.GenType, rand);
            }


            for (int x = 0; x < outdoorMap.Width; x++)
            {
                for (int z = 0; z < outdoorMap.Height; z++)
                {
                    outdoorMap.Set(x, z, CellIndex.Terrain, (short)(zoneTypeIds[x, z]));
                    outdoorMap.Set(x, z, CellIndex.Region, (short)regionIds[x, z]);

                    ZoneRegion region = outdoorMap.GetRegion(x, z);
                        
                    if (region == null || region.ZoneTypeId != zoneTypeIds[x,z])
                    {
                        _logService.Info("Mismatched Zone and Region Zone at : " + x + " -- " + z + " RZ: " + region.ZoneTypeId + " CZ: " + zoneTypeIds[x,z]);
                    }
                }
            }


            // Now remove all empty quest item detail slots.

            foreach (CrawlerMap map2 in world.Maps)
            {
                List<MapEntity> unsetQuestItems = map2.GetMapEntities(EntityTypes.QuestItem, byte.MaxValue);
                foreach (MapEntity mapEntity in unsetQuestItems)
                {
                    map2.SetEntity(mapEntity.X, mapEntity.Z, 0, 0);
                }
            }

            // Log quest items.

            foreach (CrawlerMap cmap in world.Maps)
            {
                if (cmap.MapQuestItemId > 0)
                {
                    WorldQuestItem wqi = world.QuestItems.FirstOrDefault(x => x.IdKey == cmap.MapQuestItemId);
                    if (wqi != null)
                    {
                        CrawlerMap otherMap = world.GetMap(wqi.FoundInMapId);

                    }
                }
                if (cmap.CrawlerMapTypeId != CrawlerMapTypes.Dungeon)
                {
                    await AddProps(cmap, rand);
                }
            }

            // Set city edge zone types

            foreach (CrawlerMap map in world.Maps)
            {
                if (map.CrawlerMapTypeId != CrawlerMapTypes.City)
                {
                    continue;
                }

                foreach (ZoneEdge edge in map.EdgePoints)
                {
                    MapCellDetail detail = map.Details.FirstOrDefault(d => d.EntityTypeId == EntityTypes.Map &&  Math.Abs(d.X - edge.X) + Math.Abs(d.Z - edge.Z) <= 2);

                    if (detail != null)
                    {
                        CrawlerMap otherMap = world.GetMap(detail.EntityId);

                        if (otherMap != null)
                        {
                            edge.ZoneTypeId = otherMap.Get(detail.ToX, detail.ToZ, CellIndex.Terrain);
                        }
                    }
                }
            }

            NewCrawlerMap newMap = new NewCrawlerMap()
            {
                Map = outdoorMap,
                EnterX = -1,
                EnterZ = -1,
            };
            outdoorMap.Name = "The World";

            List<MapCellDetail> allEntrances = outdoorMap.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

            int startNPCPoints = 20 * regionCount;

            int minNPCSeparation = genData.MapType.MinNpcSeparation;
            int minDistanceToEntrance = genData.MapType.MinDistanceToEntrance;

            samplingData = new SamplingData()
            {
                Count = startNPCPoints,
                MaxAttemptsPerItem = 20,
                MinX = cityDistanceFromEdge,
                MaxX = outdoorMap.Width - cityDistanceFromEdge,
                MinZ = cityDistanceFromEdge,
                MaxZ = outdoorMap.Height - cityDistanceFromEdge,
                MinSeparation = minNPCSeparation,
                Seed = rand.Next(),
            };

            SamplingResult npcResult = _samplingService.PlanePoissonSample(samplingData);

            await AddMapNpcs(party, world, genData, outdoorMap, npcResult.Points, rand);

            AddEdgeRivers(outdoorMap, rand);

            return newMap;
        }


        private async ValueTask AddProps(CrawlerMap map, IRandom rand)
        {

            ZoneTypeSettings zoneSettings = _gameData.Get<ZoneTypeSettings>(_gs.ch);

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {

                    if (map.Get(x, z, CellIndex.EntityType) > 0 || map.Get(x, z, CellIndex.EntityId) > 0)
                    {
                        continue;
                    }

                    if (map.Details.Any(xx => xx.X == x && xx.Z == z))
                    {
                        continue;
                    }

                    long centerTerrain = map.Get(x, z, CellIndex.Terrain);

                    if (map.CrawlerMapTypeId == CrawlerMapTypes.City && centerTerrain == ZoneTypes.Road)
                    {
                        continue;
                    }

                    ZoneType ztype = zoneSettings.Get(centerTerrain);



                    if (ztype != null && ztype.Props != null && ztype.Props.Count > 0)
                    {

                        if (ztype.MinSameAdjacentZone > 0)
                        {
                            int sameNearby = 0;

                            if (x < map.Width - 1 && map.Get(x + 1, z, CellIndex.Terrain) == centerTerrain)
                            {
                                sameNearby++;
                            }
                            if (x > 0 && map.Get(x - 1, z, CellIndex.Terrain) == centerTerrain)
                            {
                                sameNearby++;
                            }
                            if (z > 0 && map.Get(x, z - 1, CellIndex.Terrain) == centerTerrain)
                            {
                                sameNearby++;
                            }
                            if (z < map.Height - 1 && map.Get(x, z + 1, CellIndex.Terrain) == centerTerrain)
                            {
                                sameNearby++;
                            }

                            if (sameNearby < ztype.MinSameAdjacentZone)
                            {
                                continue;
                            }

                        }
                    }
                }
            }
            await Task.CompletedTask;
        }



        /// <summary>
        /// This method finds dungeons nearby in the outdoor map and dungeons that are in nearby cities.
        /// </summary>
        /// <param name="party"></param>
        /// <param name="world"></param>
        /// <param name="map"></param>
        /// <param name="npcDetail"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public override NpcQuestMaps GetQuestMapsForNpc(PartyData party, CrawlerWorld world, CrawlerMap map, MapCellDetail npcDetail, IRandom rand)
        {
            NpcQuestMaps maps = new NpcQuestMaps();

            CrawlerQuestSettings questSettings = _gameData.Get<CrawlerQuestSettings>(_gs.ch);

            List<MapCellDetail> entrances = map.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

            List<MapCellDetail> startNearbyEntrances = entrances.
                Where(e => MathUtil.PythagoreanDistance(npcDetail.X - e.X, npcDetail.Z - e.Z) < questSettings.MaxDistanceFromQuestGiverToTargetMap).ToList();

            foreach (MapCellDetail entrance in startNearbyEntrances)
            {
                CrawlerMap detailMap = world.GetMap(entrance.EntityId);

                if (detailMap == null)
                {
                    continue;
                }

                if (detailMap.CrawlerMapTypeId == CrawlerMapTypes.Dungeon)
                {
                    maps.PrimaryMaps.Add(new MapLink() { Map = detailMap, Link = entrance });
                }
                else if (detailMap.CrawlerMapTypeId == CrawlerMapTypes.City)
                {
                    List<MapCellDetail> cityEntrances = detailMap.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

                    foreach (MapCellDetail cityEntrance in cityEntrances)
                    {
                        CrawlerMap cityDungeonMap = world.GetMap(cityEntrance.EntityId);

                        if (cityDungeonMap != null && cityDungeonMap.CrawlerMapTypeId == CrawlerMapTypes.Dungeon)
                        {
                            maps.SecondaryMaps.Add(new MapLink() { Map = cityDungeonMap, Link = entrance });
                        }
                    }
                }
            }

            return maps;
        }

        private void AddEdgeRivers(CrawlerMap map, IRandom rand)
        {
            int edgeLength = 2 * (map.Width + map.Height);

            List<Point2I> endPoints = new List<Point2I>();

            int minRiverDist = 12;
            int maxRiverDist = 25;

            List<int> bottomPoints = GetRandomPointsWithinRange(0, map.Width, minRiverDist, maxRiverDist, rand);
            List<int> topPoints = GetRandomPointsWithinRange(0, map.Width, minRiverDist, maxRiverDist, rand);

            List<int> leftPoints = GetRandomPointsWithinRange(0, map.Height, minRiverDist, maxRiverDist, rand);
            List<int> rightPoints = GetRandomPointsWithinRange(0, map.Height, minRiverDist, maxRiverDist, rand);


            foreach (int val in bottomPoints)
            {
                AddRiver(map, val, 0, rand);
            }

            foreach (int val in topPoints)
            {
                AddRiver(map, val, map.Height - 1, rand);
            }

            foreach (int val in leftPoints)
            {
                AddRiver(map, 0, val, rand);
            }

            foreach (int val in rightPoints)
            {
                AddRiver(map, map.Width - 1, val, rand);
            }


        }

        private void AddRiver(CrawlerMap map, int startX, int startZ, IRandom rand)
        {
            int xdir = (startX == 0 ? 1 : startX == map.Width - 1 ? -1 : 0);
            int zdir = (startZ == 0 ? 1 : startZ == map.Height - 1 ? -1 : 0);

            int xsize = map.Width / 4;
            int zsize = map.Height / 4;

            int xlen = RandUtils.IntRange(xsize * 2 / 3, xsize * 4 / 3, rand);
            int zlen = RandUtils.IntRange(zsize * 2 / 3, zsize * 4 / 3, rand);

            int endX = startX + xdir * xsize + RandUtils.IntRange(-xsize / 2, xsize / 2, rand);
            int endZ = startZ + zdir * zsize + RandUtils.IntRange(-zsize / 2, zsize / 2, rand);

            if (xdir == 0)
            {
                endX += RandUtils.IntRange(xsize / 3, xsize / 2, rand) * (rand.NextDouble() < 0.5 ? 1 : -1);
            }
            if (zdir == 0)
            {
                endZ += RandUtils.IntRange(zsize / 3, zsize / 2, rand) * (rand.NextDouble() < 0.5 ? 1 : -1);
            }


            int maxLength = Math.Max(Math.Abs(endX - startX), Math.Abs(endZ - startZ));

            int minBendLength = 3;
            int maxBendLength = 8;
            int bendDelta = 1;
            int minBranchLength = 1;
            int maxBranchLength = 3;
            double branchChance = 0.1f;

            int dist = 0;
            int cx = startX;
            int cz = startZ;

            List<Point2I> points = new List<Point2I>();

            while (dist < maxLength)
            {
                dist += RandUtils.IntRange(minBendLength, maxBendLength, rand);

                if (dist > maxLength)
                {
                    dist = maxLength;
                }

                double pct = 1.0 * dist / maxLength;

                int nx = (int)(startX + (endX - startX) * pct);
                int nz = (int)(startZ + (endZ - startZ) * pct);

                nx += RandUtils.IntRange(-bendDelta, bendDelta, rand);
                nz += RandUtils.IntRange(-bendDelta, bendDelta, rand);

                points.AddRange(_lineGenService.GridConnect(cx, cz, nx, nz, rand.NextDouble() < 0.5));


                if (rand.NextDouble() < branchChance)
                {
                    int branchLength = RandUtils.IntRange(minBranchLength, maxBranchLength, rand);

                    int bex = (xdir != 0 ? 0 : 1) * branchLength;
                    int bez = (zdir != 0 ? 0 : 1) * branchLength;

                    if (rand.NextDouble() < 0.5f)
                    {
                        bex = -bex;
                    }
                    if (rand.NextDouble() < 0.5f)
                    {
                        bez = -bez;
                    }

                    points.AddRange(_lineGenService.GridConnect(nx, nz, nx + bex, nz + bez, xdir == 0));
                }
            }

            foreach (Point2I pt in points)
            {
                if (pt.X < 0 || pt.Z < 0 || pt.X >= map.Width || pt.Z >= map.Height)
                {
                    continue;
                }

                long terrain = map.Get(pt.X, pt.Z, CellIndex.Terrain);


                ZoneType ztype = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(terrain);

                if (ztype != null && (ztype.GenChance > 0 || ztype.IdKey == ZoneTypes.Mountains || ztype.IdKey == ZoneTypes.Road))
                {
                    map.Set(pt.X, pt.Z, CellIndex.Terrain, ZoneTypes.Water);
                }
            }
        }


        private List<int> GetRandomPointsWithinRange(int start, int end, int minSkip, int maxSkip, IRandom rand)
        {
            List<int> retval = new List<int>();

            int curr = start;

            while (curr < end - maxSkip)
            {
                curr += RandUtils.IntRange(minSkip, maxSkip, rand);

                if (curr < end - minSkip)
                {
                    retval.Add(curr);
                }
            }
            return retval;
        }
    }
}



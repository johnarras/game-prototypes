using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.MapGen.Entities;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Quests.Services;
using Genrpg.Shared.Crawler.Quests.Settings;
using Genrpg.Shared.Crawler.Upgrades.Constants;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.ProcGen.Services;
using Genrpg.Shared.ProcGen.Settings.Trees;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Riddles.Services;
using Genrpg.Shared.Riddles.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Constants;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.GenerateMaps
{
    public class OutdoorCrawlerMapGenHelper : BaseCrawlerMapGenHelper
    {

        private ISamplingService _samplingService = null;
        private ILootGenService _lootGenService = null;
        private IRiddleService _riddleService = null;
        private ICrawlerQuestService _questService = null;

        public override long Key => CrawlerMapTypes.Outdoors;

        public override async Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData, CancellationToken token)
        {
            IRandom rand = new MyRandom(genData.World.Seed / 4 + genData.World.MaxMapId * 131);
            int width = MathUtils.IntRange(genData.GenType.MinWidth, genData.GenType.MaxWidth, rand);
            int height = MathUtils.IntRange(genData.GenType.MinHeight, genData.GenType.MaxHeight, rand);

            height = 65;
            CrawlerMap outdoorMap = _worldService.CreateMap(genData, width, height);
            outdoorMap.ZoneTypeId = 0;
            outdoorMap.ZoneUnits = new List<ZoneUnitSpawn>();

            byte[,] overrides = new byte[outdoorMap.Width, outdoorMap.Height];
            long[,] terrain = new long[outdoorMap.Width, outdoorMap.Height];
            long[,] regionCells = new long[outdoorMap.Width, outdoorMap.Height];
            Reward[,] objects = new Reward[outdoorMap.Width, outdoorMap.Height];

            List<ZoneRegion> regions = new List<ZoneRegion>();

            List<ZoneType> allZoneTypes = _gameData.Get<ZoneTypeSettings>(null).GetData().OrderBy(x => x.MinLevel).ToList();

            List<long> okZoneIds = allZoneTypes.Where(x => x.GenChance > 0).Select(x => x.IdKey).ToList();

            int startMapEdgeSize = 4;

            int cityDistanceFromEdge = startMapEdgeSize * 2;

            int fullRegionZones = allZoneTypes.Where(x => x.MinLevel <= 100 && x.GenChance > 0).Count();

            SamplingData samplingData = new SamplingData()
            {
                Count = fullRegionZones,
                MaxAttemptsPerItem = 20,
                XMin = cityDistanceFromEdge / 4,
                XMax = outdoorMap.Width - cityDistanceFromEdge / 4,
                YMin = cityDistanceFromEdge,
                YMax = outdoorMap.Height - cityDistanceFromEdge,
                MinSeparation = 15,
                Seed = rand.Next(),
            };

            List<PointXZ> points = _samplingService.PlanePoissonSampleInteger(samplingData);

            int sortx = (rand.NextDouble() < 0.5 ? -1 : 1);
            int sorty = (rand.NextDouble() < 0.5 ? -1 : 1);

            points = points.OrderBy(p => p.X * sortx).ThenBy(p => p.Z * sorty).ToList();

            List<PointXZ> origPoints = new List<PointXZ>(points);

            PointXZ firstPoint = points[0];

            points = points.OrderBy(p =>
                Math.Sqrt(
                    (p.X - firstPoint.X) * (p.X - firstPoint.X) +
                    (p.Z - firstPoint.Z) * (p.Z - firstPoint.Z)
                    )).ToList();


            origPoints = new List<PointXZ>(points);

            int level = 1;
            int levelDelta = 7;
            float spreadDelta = 0.2f;
            float dirDelta = 0.3f;

            long cityZoneId = 0;
            long waterZoneId = allZoneTypes.FirstOrDefault(x => x.Name == "Water").IdKey;
            long roadZoneId = allZoneTypes.FirstOrDefault(x => x.Name == "Road").IdKey;
            long mountainZoneId = allZoneTypes.FirstOrDefault(x => x.Name == "Mountains").IdKey;

            List<ZoneType> startOkZones = allZoneTypes.Where(x => x.GenChance > 0 && x.IdKey != ZoneTypes.Mountains).ToList();
            while (points.Count > 0 && startOkZones.Count > 0)
            {
                List<ZoneType> okZones = startOkZones.Where(x => x.MinLevel <= level).ToList();

                if (okZones.Count < 1)
                {
                    break;
                }

                PointXZ centerPoint = points[0];

                points.Remove(centerPoint);

                ZoneType biomeType = okZones[rand.Next() % okZones.Count];

                startOkZones.Remove(biomeType);

                ZoneRegion region = new ZoneRegion()
                {
                    CenterX = (int)centerPoint.X,
                    CenterY = (int)centerPoint.Z,
                    SpreadX = MathUtils.FloatRange(1 - spreadDelta, 1 + spreadDelta, rand),
                    SpreadY = MathUtils.FloatRange(1 - spreadDelta, 1 + spreadDelta, rand),
                    ZoneTypeId = biomeType.IdKey,
                    DirX = MathUtils.FloatRange(-dirDelta, dirDelta, rand),
                    DirY = MathUtils.FloatRange(-dirDelta, dirDelta, rand),
                    Level = level,
                };

                level += levelDelta;

                regions.Add(region);

            }

            outdoorMap.LevelDelta = level + levelDelta;

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
                    for (int y = 0; y < outdoorMap.Height; y++)
                    {
                        if (terrain[x, y] == 0)
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

                foreach (ZoneRegion region in regions)
                {
                    region.Name = _zoneGenService.GenerateZoneName(region.ZoneTypeId, rand.Next(), false);
                    float currRadius = MathUtils.FloatRange(radius * (1 - radiusDelta), radius * (1 + radiusDelta), rand);

                    float xrad = currRadius * region.SpreadX;
                    float yrad = currRadius * region.SpreadY;
                    float xcenter = region.CenterX + region.DirX * currRadius;
                    float ycenter = region.CenterY * region.DirY * currRadius;

                    xcenter = region.CenterX;
                    ycenter = region.CenterY;

                    int xmin = MathUtils.Clamp(0, (int)(xcenter - xrad - 1), outdoorMap.Width - 1);
                    int xmax = MathUtils.Clamp(0, (int)(xcenter + xrad + 1), outdoorMap.Width - 1);

                    int ymin = MathUtils.Clamp(0, (int)(ycenter - yrad - 1), outdoorMap.Height - 1);
                    int ymax = MathUtils.Clamp(0, (int)(ycenter + yrad + 1), outdoorMap.Height - 1);

                    for (int x = xmin; x <= xmax; x++)
                    {
                        for (int y = ymin; y <= ymax; y++)
                        {

                            if (terrain[x, y] != 0)
                            {
                                continue;
                            }

                            float xpct = (x - xcenter) / xrad;
                            float ypct = (y - ycenter) / yrad;

                            float distScale = Mathf.Sqrt(xpct * xpct + ypct * ypct);

                            if (distScale <= 1)
                            {
                                terrain[x, y] = region.ZoneTypeId;
                                regionCells[x, y] = region.ZoneTypeId;
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
                cornerRadii.Add(MathUtils.FloatRange(minCornerRadius, maxCornerRadius, rand));
            }

            int maxCheckRadius = (int)(maxCornerRadius + startMapEdgeSize);

            int xcorner = 0;
            int ycorner = 0;
            for (int x = 0; x < outdoorMap.Width; x++)
            {

                for (int y = 0; y < outdoorMap.Height; y++)
                {
                    int cornerIndex = -1;

                    if (x <= maxCheckRadius)
                    {
                        xcorner = 0;
                        if (y <= maxCheckRadius)
                        {
                            ycorner = 0;
                            cornerIndex = 0;
                        }
                        else if (y >= outdoorMap.Height - maxCheckRadius - 1)
                        {
                            cornerIndex = 1;
                            ycorner = outdoorMap.Height - 1;
                        }
                    }
                    else if (x >= outdoorMap.Width - maxCheckRadius - 1)
                    {
                        xcorner = outdoorMap.Width - 1;
                        if (y <= maxCheckRadius)
                        {
                            cornerIndex = 2;
                            ycorner = 0;
                        }
                        else if (y >= outdoorMap.Height - maxCheckRadius - 1)
                        {
                            cornerIndex = 3;
                            ycorner = outdoorMap.Height - 1;
                        }
                    }

                    int mapEdgeSize = startMapEdgeSize + MathUtils.IntRange(-1, 1, rand);
                    if ((x < mapEdgeSize || x >= outdoorMap.Width - mapEdgeSize) ||
                        (y < mapEdgeSize || y >= outdoorMap.Height - mapEdgeSize))
                    {
                        terrain[x, y] = waterZoneId;
                    }


                    if (cornerIndex >= 0 && cornerIndex < cornerRadii.Count)
                    {
                        int currRadius = (int)cornerRadii[cornerIndex] + startMapEdgeSize;


                        int cx = xcorner;
                        int cy = ycorner;

                        if (cx > 0)
                        {
                            cx -= currRadius;
                        }
                        else
                        {
                            cx += currRadius;
                        }

                        if (cy > 0)
                        {
                            cy -= currRadius;

                        }
                        else
                        {
                            cy += currRadius;
                        }

                        if (cx < outdoorMap.Width / 2 && x > cx)
                        {
                            continue;
                        }

                        if (cx > outdoorMap.Width / 2 && x < cx)
                        {
                            continue;
                        }

                        if (cy < outdoorMap.Height / 2 && y > cy)
                        {
                            continue;
                        }

                        if (cy > outdoorMap.Height / 2 && y < cy)
                        {
                            continue;
                        }

                        float currDist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));

                        currDist += MathUtils.FloatRange(-1, 1, rand);

                        if (currDist >= currRadius && terrain[x, y] != waterZoneId)
                        {
                            terrain[x, y] = waterZoneId;
                        }
                    }
                }
            }
            // Roads between cities

            AddRoadsBetweenCities(outdoorMap, origPoints, terrain, rand);

            // Mountains at zone borders. (okZoneIds if  two diff make a small blob...only replacing things in ok biomeIds

            int crad = 1;
            int rrad = 2;
            int trad = Math.Max(crad, rrad);
            for (int x = trad; x < outdoorMap.Width - trad; x++)
            {
                for (int y = trad; y < outdoorMap.Height - trad; y++)
                {
                    List<long> currOkZoneIds = new List<long>();
                    bool nearRoad = false;

                    // Check for roads.
                    for (int xx = x - rrad; xx <= x + rrad; xx++)
                    {
                        for (int yy = y - rrad; yy <= y + rrad; yy++)
                        {
                            if (terrain[xx, yy] == roadZoneId)
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
                        for (int yy = y - crad; yy <= y + crad; yy++)
                        {
                            long tid = regionCells[xx, yy];
                            if (tid != mountainZoneId && okZoneIds.Contains(tid))
                            {
                                if (!currOkZoneIds.Contains(tid))
                                {
                                    currOkZoneIds.Add(tid);
                                }
                            }
                        }
                    }

                    int nrad = rand.NextDouble() < 0.2f ? 1 : 0;

                    if (currOkZoneIds.Count > 1)
                    {
                        for (int xx = x - nrad; xx <= x + nrad; xx++)
                        {
                            for (int yy = y - nrad; yy <= y + nrad; yy++)
                            {
                                terrain[xx, yy] = mountainZoneId;
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < outdoorMap.Width; x++)
            {
                for (int y = 0; y < outdoorMap.Height; y++)
                {
                    outdoorMap.Set(x, y, CellIndex.Terrain, (short)(terrain[x, y]));
                    outdoorMap.Set(x, y, CellIndex.Region, (short)regionCells[x, y]);
                }
            }

            ZoneType cityZoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(ZoneTypes.City);

            for (int c = 0; c < origPoints.Count; c++)
            {
                PointXZ pt = origPoints[c];

                int cityLevel = 1;
                ZoneRegion zoneRegion = regions.FirstOrDefault(x => x.CenterX == (int)pt.X && x.CenterY == (int)pt.Z);

                if (zoneRegion != null)
                {
                    cityLevel = (int)zoneRegion.Level;
                }

                terrain[(int)pt.X, (int)pt.Z] = cityZoneId;
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
                int yy = (int)pt.Z;

                int dx = 0;
                int dy = 0;

                if (outdoorMap.Get(xx, yy + 1, CellIndex.Terrain) == ZoneTypes.Road)
                {
                    dx = 0;
                    dy = 1;
                }
                else if (outdoorMap.Get(xx, yy - 1, CellIndex.Terrain) == ZoneTypes.Road)
                {
                    dx = 0;
                    dy = -1;
                }
                else if (outdoorMap.Get(xx - 1, yy, CellIndex.Terrain) == ZoneTypes.Road)
                {
                    dx = -1;
                    dy = 0;
                }
                else if (outdoorMap.Get(xx + 1, yy, CellIndex.Terrain) == ZoneTypes.Road)
                {
                    dx = 1;
                    dy = 0;
                }

                int dirAngle = DirUtils.DirDeltaToAngle(dx, dy);

                outdoorMap.Set(xx, yy, CellIndex.Dir, dirAngle / CrawlerMapConstants.DirToAngleMult);

                CrawlerMap cityMap = await _mapGenService.Generate(party, world, cityGenData, token);

                cityMap.FromPlaceName = outdoorMap.GetName(xx, yy);

                cityLevel += levelDelta;
            }

            // Add random dungeons and stuff on the map
            samplingData = new SamplingData()
            {
                Count = outdoorMap.Width * outdoorMap.Height / 150,
                MaxAttemptsPerItem = 20,
                XMin = cityDistanceFromEdge,
                XMax = outdoorMap.Width - cityDistanceFromEdge,
                YMin = cityDistanceFromEdge,
                YMax = outdoorMap.Height - cityDistanceFromEdge,
                MinSeparation = 10,
                Seed = rand.Next(),
            };

            List<PointXZ> startDungeonPoints = _samplingService.PlanePoissonSampleInteger(samplingData);

            List<PointXZ> finalDungeonPoints = new List<PointXZ>();

            double minDistFromCity = 8;

            int dungeonAttempts = startDungeonPoints.Count;
            int dungeonSuccess = 0;
            foreach (PointXZ p in startDungeonPoints)
            {
                int xx = (int)p.X;
                int yy = (int)p.Z;

                if (!okZoneIds.Contains(outdoorMap.Get(xx, yy, CellIndex.Terrain)))
                {
                    continue;
                }

                bool tooCloseToCity = false;
                foreach (ZoneRegion region in outdoorMap.Regions)
                {
                    double ddx = region.CenterX - xx;
                    double ddy = region.CenterY - yy;

                    if (Math.Sqrt(ddx * ddx + ddy * ddy) < minDistFromCity)
                    {
                        tooCloseToCity = true;
                        break;
                    }
                }

                if (tooCloseToCity)
                {
                    continue;
                }

                long dungeonLevel = 2 + await _worldService.GetMapLevelAtPoint(world, outdoorMap.IdKey, xx, yy) * 5 / 4;
                CrawlerMapGenData dungeonGenData = new CrawlerMapGenData()
                {
                    World = genData.World,
                    MapTypeId = CrawlerMapTypes.Dungeon,
                    Level = (int)dungeonLevel,
                    FromMapId = outdoorMap.IdKey,
                    FromMapX = xx,
                    FromMapZ = yy,
                };

                CrawlerMap dungeonMap = await _mapGenService.Generate(party, world, dungeonGenData, token);

                dungeonSuccess++;

                finalDungeonPoints.Add(new PointXZ(xx, yy));

                outdoorMap.SetEntity(xx, yy, EntityTypes.Building, dungeonMap.BuildingTypeId);
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

            int gameDungeonUnlockLevel = Math.Max(mapSettings.MinQuestUnlockDungeonLevel,
                party.GetUpgradePointsLevel(UpgradeReasons.CompleteDungeon, true));

            for (int dungeonIndex = 0; dungeonIndex < dungeonMapGroups.Count; dungeonIndex++)
            {
                List<CrawlerMap> floors = dungeonMapGroups[dungeonIndex];

                List<long> floorIds = floors.Select(x => x.IdKey).ToList();

                CrawlerMap entranceMap = floors.First();

                if (entranceMap.Level >= gameDungeonUnlockLevel && rand.NextDouble() < mapSettings.QuestItemEntranceUnlockChance)
                {
                    string questItemName = _lootGenService.GenerateItemNames(rand, 1, 100).First().SingularName;

                    int lookbackDistance = 6;

                    List<int> okIndexes = new List<int>();

                    for (int i = dungeonIndex - 1; i >= 0 && dungeonIndex - i <= lookbackDistance + 1; i--)
                    {
                        if (dungeonMapGroups[i].Any(x => x.Level >= entranceMap.Level) ||
                            !dungeonMapGroups[i].Any(x => x.Level >= mapSettings.MinQuestItemDungeonLevel))
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
                            questItemMap.ZoneUnits = questItemMap.ZoneUnits.OrderBy(x => HashUtils.NewUUId()).ToList();
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
            }

            // Now random trees. (1000 + building Id vs building id)

            IReadOnlyList<TreeType> treeTypes = _gameData.Get<TreeTypeSettings>(null).GetData();

            for (int x = 0; x < outdoorMap.Width; x++)
            {
                for (int z = 0; z < outdoorMap.Height; z++)
                {
                    if (outdoorMap.GetEntityId(x, z, EntityTypes.Building) > 0)
                    {
                        continue;
                    }
                    ZoneType ztype = allZoneTypes.FirstOrDefault(t => t.IdKey == outdoorMap.Get(x, z, CellIndex.Terrain));
                    if (ztype != null && ztype.TreeTypes != null && ztype.TreeTypes.Count > 0)
                    {
                        double chance = ztype.TreeDensity * CrawlerMapConstants.TreeChanceScale;

                        if (rand.NextDouble() < chance)
                        {
                            long treeTypeId = 0;
                            for (int tries = 0; tries < 20; tries++)
                            {
                                ZoneTreeType zttype = ztype.TreeTypes[rand.Next() % ztype.TreeTypes.Count];
                                TreeType ttype = treeTypes.FirstOrDefault(x => x.IdKey == zttype.TreeTypeId);
                                if (!ttype.HasFlag(TreeFlags.IsBush))
                                {
                                    treeTypeId = ztype.IdKey;
                                    break;
                                }
                            }

                            if (treeTypeId > 0)
                            {
                                outdoorMap.SetEntity(x, z, EntityTypes.Tree, treeTypeId);
                                outdoorMap.Set(x, z, CellIndex.Dir, _crawlerMapService.GetMapCellHash(outdoorMap.IdKey, x, z, 77) % 4);
                            }
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

            int startNPCPoints = 20 * fullRegionZones;

            int minNPCSeparation = genData.MapType.MinNpcSeparation;
            int minDistanceToEntrance = genData.MapType.MinDistanceToEntrance;

            samplingData = new SamplingData()
            {
                Count = startNPCPoints,
                MaxAttemptsPerItem = 20,
                XMin = cityDistanceFromEdge,
                XMax = outdoorMap.Width - cityDistanceFromEdge,
                YMin = cityDistanceFromEdge,
                YMax = outdoorMap.Height - cityDistanceFromEdge,
                MinSeparation = minNPCSeparation,
                Seed = rand.Next(),
            };

            List<PointXZ> npcPoints = _samplingService.PlanePoissonSampleInteger(samplingData);

            await AddMapNpcs(party, world, genData, outdoorMap, npcPoints, rand);

            await _questService.AddWorldQuestGivers(party, world, rand, token);

            return newMap;
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
                Where(e => MathUtils.PythagoreanDistance(npcDetail.X - e.X, npcDetail.Z - e.Z) < questSettings.MaxDistanceFromQuestGiverToTargetMap).ToList();

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

        private void AddRoadsBetweenCities(CrawlerMap map, List<PointXZ> cityLocs, long[,] terrain, IRandom rand)
        {
            List<PointXZ> remainingPoints = new List<PointXZ>(cityLocs);

            List<ConnectPointData> cityPoints = new List<ConnectPointData>();

            int centerId = 0;
            foreach (PointXZ cityLoc in cityLocs)
            {
                ConnectPointData connectionData = new ConnectPointData()
                {
                    Id = ++centerId,
                    X = cityLoc.X,
                    Z = cityLoc.Z,
                    Data = cityLoc,
                    MaxConnections = 3,
                };
                cityPoints.Add(connectionData);
            }

            List<ConnectedPairData> roadsToMake = _lineGenService.ConnectPoints(cityPoints, rand, 0.0f);

            foreach (ConnectedPairData pairData in roadsToMake)
            {

                PointXZ start = new PointXZ((int)pairData.Point1.X, (int)pairData.Point1.Z);
                PointXZ end = new PointXZ((int)pairData.Point2.X, (int)pairData.Point2.Z);

                double totalDistance = MathUtils.PythagoreanDistance(start.X - end.X, start.Z - end.Z);

                int intDistance = (int)(totalDistance);

                int posDelta = intDistance / 3;

                int midPointQuantity = 0;

                float midPointChance = 0.5f;
                int mapEdgeSize = 6;
                while (rand.NextDouble() < midPointChance && midPointQuantity < totalDistance / 4)
                {
                    midPointQuantity++;
                }

                List<PointXZ> points = new List<PointXZ>();
                points.Add(start);

                for (int i = 0; i < midPointQuantity; i++)
                {
                    float percent = MathUtils.FloatRange(0, 1, rand);

                    int fx = (int)(start.X + (end.X - start.X) * percent);
                    int fz = (int)(start.Z + (end.Z - start.Z) * percent);

                    fx += MathUtils.IntRange(-posDelta, posDelta, rand);
                    fz += MathUtils.IntRange(-posDelta, posDelta, rand);

                    fx = MathUtils.Clamp(mapEdgeSize, fx, map.Width - mapEdgeSize);
                    fz = MathUtils.Clamp(mapEdgeSize, fz, map.Height - mapEdgeSize);

                    if (MathUtils.PythagoreanDistance(fx - start.X, fz - start.Z) >= totalDistance)
                    {
                        continue;
                    }

                    points.Add(new PointXZ(fx, fz));
                }

                points = points.OrderBy(p => MathUtils.PythagoreanDistance(p.X - start.X, p.Z - start.Z)).ToList();



                points.Add(end);

                for (int p = 0; p < points.Count - 1; p++)
                {
                    int csx = points[p].X;
                    int csz = points[p].Z;

                    int cex = points[p + 1].X;
                    int cez = points[p + 1].Z;

                    terrain[csx, csz] = ZoneTypes.Road;
                    terrain[cex, cez] = ZoneTypes.Road;

                    int cmx = csx;
                    int cmz = cez;

                    if (rand.NextDouble() < 0.5)
                    {
                        cmx = cex;
                        cmz = csz;
                    }

                    for (int xx = csx; xx != cex; xx += Math.Sign(cex - csx))
                    {
                        terrain[xx, cmz] = ZoneTypes.Road;
                    }
                    for (int zz = csz; zz != cez; zz += Math.Sign(cez - csz))
                    {
                        terrain[cmx, zz] = ZoneTypes.Road;
                    }
                }
            }
        }
    }
}

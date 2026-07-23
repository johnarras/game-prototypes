using OxDb.Client.Crawler.MapGen.Helpers;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Buildings.Settings;
using OxDb.SharedGame.Crawler.MapGen.Entities;
using OxDb.SharedGame.Crawler.MapGen.Helpers;
using OxDb.SharedGame.Crawler.MapGen.Settings;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Maps.Settings;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Options.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.ProcGen.Entities;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Units.Settings;
using OxDb.SharedGame.Zones.Constants;
using OxDb.SharedGame.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace OxDb.Client.Crawler.MapGen.Services
{

    public interface ICrawlerMapGenService : IInitializable
    {
        ICrawlerMapGenHelper GetGenHelper(long mapType);
        Task<CrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData, CancellationToken token);
        void OneWayLink(CrawlerWorld world, long fromMapId, int fromX, int fromZ, long toMapId, int toX, int toZ);
        Task<CrawlerMap> GenerateRoguelikeDungeonLevel(PartyData party, CrawlerWorld world, long mapId, int enterX, int enterZ, CancellationToken token);
        bool RoomAreaIsBlank(int[,] roomIds, int minx, int maxx, int minz, int maxz);
        void RemoveInnerWallsFromOutdoorDungeons(CrawlerMap map);
        void AddMapBoundaryWalls(CrawlerMap map);
        void AddBoundaryWallsAtPoint(CrawlerMap map, int x, int z);

        void AddPathsBetweenPoints(CrawlerMap map, List<SampledPoint> locs, int mapEdgeSize, List<long> skipZoneTypeIds, long newZoneTypeId, IRandom rand);

        void ConnectPairOfPoints(CrawlerMap map, ConnectedPairData pairData, int mapEdgeSize,
            List<long> skipZoneTypeIds, long newZoneTypeId,
            IRandom rand);
        void RemoveEdgePoints(CrawlerMap map, int edgeDistance);
        void SetDungeonEntranceAndExitPoints(CrawlerMap map, DungeonLevelGenArgs levelArgs);

        void AddRoomWithDoor(DungeonLevelGenArgs levelArgs, int x, int z, EMapDirs doorDir, long zoneTypeId);

        void RemoveDisconnectedComponents(CrawlerMap map);

        void AddSmallRoomsAndBlankSpaces(DungeonLevelGenArgs levelArgs);

        void SetWallBitsFromDeltas(CrawlerMap map, int x, int z, int dx, int dz, int wallType);

        void AddOutdoorDungeonZoneEdges(CrawlerMap map);
    }

    public class CrawlerMapGenService : ICrawlerMapGenService
    {
        private ILogService _logService = null;
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private ICrawlerMapService _mapService = null;
        private ICrawlerOptionsService _optionsService = null;
        private CancellationToken _token;
        private ILineGenService _lineGenService = null;

        private SetupDictionaryContainer<long, ICrawlerMapGenHelper> _mapGenHelpers = new SetupDictionaryContainer<long, ICrawlerMapGenHelper>();

        public async Task Initialize(CancellationToken token)
        {

            _token = token;

            await Task.CompletedTask;
        }
        public ICrawlerMapGenHelper GetGenHelper(long mapType)
        {
            if (_mapGenHelpers.TryGetValue(mapType, out ICrawlerMapGenHelper helper))
            {
                return helper;
            }
            return null;
        }

        public async Task<CrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData, CancellationToken token)
        {
            CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);
            CrawlerMapType mtype = mapSettings.Get(genData.MapTypeId);

            if (mtype == null)
            {
                return null;
            }

            IRandom rand = new MyRandom(world.GetMaxMapId() + 3 + world.Seed / 3);

            genData.MapType = mtype;
            if (genData.GenType == null)
            {
                genData.GenType = RandUtils.GetRandomElement(mtype.GenTypes, rand);
            }

            if (genData.MapType.ForcedZoneTypeId > 0)
            {
                genData.ZoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(genData.MapType.ForcedZoneTypeId);
            }

            if (genData.ZoneType == null)
            {

                bool isOutdoorMap = rand.NextDouble() < mapSettings.OutdoorDungeonChance;
                isOutdoorMap = true;
                List<ZoneType> zoneTypes = _gameData.Get<ZoneTypeSettings>(_gs.ch).GetData().Where(x => x.IsDungeon && x.IsOutdoors == isOutdoorMap).ToList();

                genData.ZoneType = zoneTypes[rand.Next() % zoneTypes.Count];

                int keywordCount = 0;
                while (rand.NextDouble() < mapSettings.UnitKeywordChance && keywordCount < 3)
                {
                    keywordCount++;
                }

                keywordCount = 1;

                if (keywordCount > 0)
                {
                    List<ZoneUnitKeyword> zoneKeywords = genData.ZoneType.UnitKeyWords.ToList();

                    while (keywordCount > 0 && zoneKeywords.Count > 0)
                    {
                        ZoneUnitKeyword zk = RandUtils.GetRandomElement(zoneKeywords, rand);
                        UnitKeyword uk = _gameData.Get<UnitKeywordSettings>(_gs.ch).Get(zk.UnitKeywordId);

                        if (uk != null)
                        {
                            genData.UnitKeywords.Add(new CurrentUnitKeyword() { UnitKeywordId = uk.IdKey });
                        }
                        zoneKeywords.Remove(zk);
                        keywordCount--;
                    }
                }
            }

            if (genData.BuildingArtId == 0)
            {
                genData.BuildingArtId = RandUtils.GetRandomElement(_gameData.Get<BuildingArtSettings>(_gs.ch).GetData(), rand).IdKey;
            }

            if (genData.ArtSeed == 0)
            {
                genData.ArtSeed = _gs.Rand.Next(1000000000); // Use global rand here to make it random each time we generate
            }

            ICrawlerMapGenHelper helper = GetGenHelper(genData.MapType.IdKey);
            NewCrawlerMap newMap = await helper.Generate(party, world, genData, token);

            if (newMap == null || newMap.Map == null)
            {
                _logService.Info("NullMap? " + genData.MapType.IdKey);
                return null;
            }

            SetObjectDirections(newMap.Map, rand);

            if (_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                if (genData.FromMapId > 0 && newMap.EnterX >= 0 && newMap.EnterZ >= 0)
                {
                    LinkTwoMaps(world, genData.FromMapId, genData.FromMapX, genData.FromMapZ, newMap.Map.IdKey, newMap.EnterX, newMap.EnterZ);
                }
            }


            ZoneTypeSettings zoneSettings = _gameData.Get<ZoneTypeSettings>(_gs.ch);

            for (int x = 0; x < newMap.Map.Width; x++)
            {
                for (int z = 0; z < newMap.Map.Height; z++)
                {
                    ZoneType ztype = zoneSettings.Get(newMap.Map.Get(x, z, CellIndex.Terrain));

                    if (ztype != null)
                    {
                        if (!newMap.Map.IsOutdoorDungeon())
                        {
                            long entityTypeId = newMap.Map.Get(x, z, CellIndex.EntityType);
                            long entityId = newMap.Map.Get(x, z, CellIndex.EntityId);

                            if (entityTypeId == 0 && entityId == 0)
                            {
                                if (rand.NextDouble() < ztype.LargePropChance)
                                {
                                    newMap.Map.SetEntity(x, z, EntityTypes.Prop, 1);
                                }
                            }
                        }
                    }
                    else if (newMap.Map.IsOutdoorDungeon())
                    {
                        newMap.Map.SetEntity(x, z, EntityTypes.Prop, 1);
                    }
                }
            }

            AddOutdoorDungeonZoneEdges(newMap.Map);

            return newMap.Map;
        }

        private void LinkTwoMaps(CrawlerWorld world, long fromMapId, int fromMapX, int fromMapZ, long toMapId, int toMapX, int toMapZ)
        {
            OneWayLink(world, fromMapId, fromMapX, fromMapZ, toMapId, toMapX, toMapZ);
            OneWayLink(world, toMapId, toMapX, toMapZ, fromMapId, fromMapX, fromMapZ);
        }

        public void OneWayLink(CrawlerWorld world, long fromMapId, int fromX, int fromZ, long toMapId, int toX, int toZ)
        {
            CrawlerMap fromMap = world.GetMap(fromMapId);
            CrawlerMap toMap = world.GetMap(toMapId);

            if (fromMap == null || toMap == null)
            {
                return;
            }

            List<Point2I> nearbyRoads = new List<Point2I>();

            for (int xx = toX - 1; xx <= toX + 1; xx++)
            {
                if (xx < 0 || xx > toMap.Width)
                {
                    continue;
                }
                for (int zz = toZ - 1; zz <= toZ + 1; zz++)
                {
                    if (zz < 0 || zz >= toMap.Height)
                    {
                        continue;
                    }

                    if (Math.Abs(xx - toX) + Math.Abs(zz - toZ) != 1)
                    {
                        continue;
                    }

                    if (toMap.Get(xx, zz, CellIndex.Terrain) != ZoneTypes.Road)
                    {
                        continue;
                    }
                    nearbyRoads.Add(new Point2I(xx, zz));
                }
            }

            List<Point2I> innerRoads = nearbyRoads.Where(r => r.X > 0 && r.Z > 0 && r.X < toMap.Width - 1 && r.Z < toMap.Height - 1).ToList();

            if (innerRoads.Count > 0)
            {
                nearbyRoads = innerRoads;
            }

            if (nearbyRoads.Count > 0)
            {

                long index = fromMapId + toMapId + fromX + fromZ + toX + toZ;
                Point2I chosenRoad = nearbyRoads[(int)(index % nearbyRoads.Count)];

                toX = (int)chosenRoad.X;
                toZ = (int)chosenRoad.Z;
            }

            if (fromX < 0 || fromZ < 0
                || fromX >= fromMap.Width || fromZ >= fromMap.Height ||
                toX < 0 || toZ < 0
                || toX >= toMap.Width || toZ >= toMap.Height)
            {
                return;
            }

            MapCellDetail currentDetail = fromMap.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map && x.EntityId == toMapId);

            if (currentDetail == null)
            {
                currentDetail = new MapCellDetail() { EntityTypeId = EntityTypes.Map, EntityId = toMapId };
                fromMap.Details.Add(currentDetail);
            }
            currentDetail.X = fromX;
            currentDetail.Z = fromZ;
            currentDetail.ToX = toX;
            currentDetail.ToZ = toZ;
            //fromMap.SetEntity(currentDetail.X, currentDetail.Z, 0, 0);

            for (int xx = fromX - 1; xx <= fromX + 1; xx++)
            {
                if (xx < 0 || xx >= fromMap.Width)
                {
                    continue;
                }
                for (int zz = fromZ - 1; zz <= fromZ + 1; zz++)
                {
                    if (zz < 0 || zz >= fromMap.Height)
                    {
                        continue;
                    }
                    if (fromMap.GetEntityId(xx, zz, EntityTypes.MapEncounter) > 0)
                    {
                        fromMap.SetEntity(xx, zz, 0, 0);
                    }
                }
            }

            foreach (ZoneEdge edge in fromMap.EdgePoints)
            {
                int dist = Math.Abs(edge.X - fromX) + Math.Abs(edge.Z - fromZ);
                if (dist <= 1)
                {
                    edge.ZoneTypeId = toMap.ZoneTypeId;
                }
            }
        }

        private void SetObjectDirections(CrawlerMap map, IRandom rand)
        {
            Dictionary<EMapDirs, MapDir> dirs = MapDirUtils.GetDirs();

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    if (map.GetEntityId(x, z, EntityTypes.Building) > 0)
                    {
                        continue;
                    }

                    List<int> allBlockingBits = new List<int>();

                    foreach (MapDir dir in dirs.Values)
                    {
                        allBlockingBits.Add(_mapService.GetBlockingBits(map, x, z, x + dir.DX, z + dir.DZ, false));
                    }

                    List<int> openDirs = new List<int>();
                    List<int> doorDirs = new List<int>();

                    for (int d = 0; d < allBlockingBits.Count; d++)
                    {
                        if (allBlockingBits[d] == WallTypes.None)
                        {
                            openDirs.Add(d);
                        }
                        else if (allBlockingBits[d] == WallTypes.Door)
                        {
                            doorDirs.Add(d);
                        }
                    }

                    if (openDirs.Count > 0)
                    {
                        map.Set(x, z, CellIndex.Dir, openDirs[_gs.Rand.Next(openDirs.Count)]);
                    }
                    else if (doorDirs.Count > 0)
                    {
                        map.Set(x, z, CellIndex.Dir, doorDirs[_gs.Rand.Next(doorDirs.Count)]);
                    }
                }
            }
        }

        public static int DirDeltaToAngle(int dx, int dy)
        {
            if (dy > 0)
            {
                return 0;
            }
            else if (dy < 0)
            {
                return 180;
            }
            else if (dx > 0)
            {
                return 90;
            }
            else if (dx < 0)
            {
                return 270;
            }
            return 0;
        }

        public async Task<CrawlerMap> GenerateRoguelikeDungeonLevel(PartyData party, CrawlerWorld world, long mapId, int enterX, int enterZ, CancellationToken token)
        {

            // Failsafe never regenerate city in roguelike game.
            if (mapId == 1)
            {
                return world.GetMap(1);
            }

            CrawlerMapGenData genData = new CrawlerMapGenData()
            {
                FromMapId = mapId - 1,
                CurrFloor = (int)mapId - 1,
                MaxFloor = (int)mapId,
                Level = mapId - 1,
                MapTypeId = CrawlerMapTypes.Dungeon,
                World = world,
                FromMapX = enterX,
                FromMapZ = enterZ,
                ForcedIdKey = mapId,
            };

            return await Generate(party, world, genData, token);
        }


        public bool RoomAreaIsBlank(int[,] roomIds, int minx, int maxx, int minz, int maxz)
        {
            if (minx < 1 || maxx >= roomIds.GetLength(0) - 1 || minz < 1 || maxz >= roomIds.GetLength(1) - 1)
            {
                return false;
            }

            for (int x = minx - 1; x <= maxx + 1; x++)
            {
                for (int z = minz - 1; z <= maxz + 1; z++)
                {
                    if (roomIds[x, z] > 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void AddMapBoundaryWalls(CrawlerMap map)
        {
            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    if (map.Get(x, z, CellIndex.Terrain) == 0)
                    {
                        continue;
                    }
                    AddBoundaryWallsAtPoint(map, x, z);
                }
            }
        }

        public void RemoveInnerWallsFromOutdoorDungeons(CrawlerMap map)
        {

            if (!map.IsOutdoorDungeon())
            {
                return;
            }

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    map.Set(x, z, CellIndex.Walls, 0);

                    if (map.Get(x, z, CellIndex.Terrain) > 0)
                    {
                        if (map.GetEntityId(x, z, EntityTypes.Prop) > 0)
                        {
                            map.SetEntity(x, z, 0, 0);
                        }
                    }
                    else
                    {
                        map.SetEntity(x, z, EntityTypes.Prop, 1);
                    }
                }
            }
        }

        public void AddBoundaryWallsAtPoint(CrawlerMap map, int x, int z)
        {

            CheckCellDir(map, x, z, x + 1, z);
            CheckCellDir(map, x, z, x - 1, z);
            CheckCellDir(map, x, z, x, z - 1);
            CheckCellDir(map, x, z, x, z + 1);
        }

        private void CheckCellDir(CrawlerMap map, int x, int z, int nx, int nz)
        {
            int dx = nx - x;
            int dz = nz - z;

            if (dx != 0)
            {
                bool placeWall = false;
                if (nx < 0)
                {
                    placeWall = true;
                }
                if (nx >= map.Width)
                {
                    placeWall = true;
                }

                if (!placeWall)
                {
                    if (map.Get(nx, z, CellIndex.Terrain) == 0)
                    {
                        placeWall = true;
                    }
                }
                if (placeWall)
                {
                    int wx = (dx < 0 ? nx : x);
                    wx = (wx + map.Width) % map.Width;
                    int wz = z;
                    map.AddBits(wx, wz, CellIndex.Walls, (WallTypes.Wall << MapWallBits.EWallStart));
                }
            }

            if (dz != 0)
            {
                bool placeWall = false;
                if (nz < 0)
                {
                    placeWall = true;
                }
                if (nz >= map.Height)
                {
                    placeWall = true;
                }

                if (!placeWall)
                {
                    if (map.Get(x, nz, CellIndex.Terrain) == 0)
                    {
                        placeWall = true;
                    }
                }
                if (placeWall)
                {
                    int wx = x;
                    int wz = (dz < 0 ? nz : z);
                    wz = (wz + map.Height) % map.Height;

                    map.AddBits(wx, wz, CellIndex.Walls, (WallTypes.Wall << MapWallBits.NWallStart));
                }
            }
        }


        private void CheckChangeTerrain(CrawlerMap map, int x, int z, List<long> skipZoneTypeIds, long newZoneTypeId)
        {
            long zoneTypeId = map.Get(x, z, CellIndex.Terrain);

            if (!skipZoneTypeIds.Contains(zoneTypeId))
            {
                map.Set(x, z, CellIndex.Terrain, newZoneTypeId);
            }
        }

        public void ConnectPairOfPoints(CrawlerMap map, ConnectedPairData pairData, int mapEdgeSize,
            List<long> skipZoneTypeIds, long newZoneTypeId,
            IRandom rand)
        {

            Point2I start = new Point2I((int)pairData.Point1.X, (int)pairData.Point1.Z);
            Point2I end = new Point2I((int)pairData.Point2.X, (int)pairData.Point2.Z);

            double totalDistance = MathUtil.PythagoreanDistance(start.X - end.X, start.Z - end.Z);

            int intDistance = (int)(totalDistance);

            int posDelta = intDistance / 3;

            int midPointQuantity = 0;

            float midPointChance = 0.5f;
            while (rand.NextDouble() < midPointChance && midPointQuantity < totalDistance / 4)
            {
                midPointQuantity++;
            }

            List<Point2I> points = new List<Point2I>();
            points.Add(start);

            for (int i = 0; i < midPointQuantity; i++)
            {
                float percent = RandUtils.FloatRange(0, 1, rand);

                int fx = (int)(start.X + (end.X - start.X) * percent);
                int fz = (int)(start.Z + (end.Z - start.Z) * percent);

                fx += RandUtils.IntRange(-posDelta, posDelta, rand);
                fz += RandUtils.IntRange(-posDelta, posDelta, rand);

                fx = MathUtil.Clamp(mapEdgeSize, fx, map.Width - mapEdgeSize);
                fz = MathUtil.Clamp(mapEdgeSize, fz, map.Height - mapEdgeSize);

                if (MathUtil.PythagoreanDistance(fx - start.X, fz - start.Z) >= totalDistance)
                {
                    continue;
                }

                points.Add(new Point2I(fx, fz));
            }

            points = points.OrderBy(p => MathUtil.PythagoreanDistance(p.X - start.X, p.Z - start.Z)).ToList();

            points.Add(end);

            for (int p = 0; p < points.Count - 1; p++)
            {
                int csx = points[p].X;
                int csz = points[p].Z;

                int cex = points[p + 1].X;
                int cez = points[p + 1].Z;


                CheckChangeTerrain(map, csx, csz, skipZoneTypeIds, newZoneTypeId);
                CheckChangeTerrain(map, cex, cez, skipZoneTypeIds, newZoneTypeId);

                int cmx = csx;
                int cmz = cez;

                if (rand.NextDouble() < 0.5)
                {
                    cmx = cex;
                    cmz = csz;
                }

                if (csx == cex)
                {
                    CheckChangeTerrain(map, csx, cmz, skipZoneTypeIds, newZoneTypeId);
                }
                else
                {
                    for (int xx = csx; xx != cex; xx += Math.Sign(cex - csx))
                    {
                        CheckChangeTerrain(map, xx, cmz, skipZoneTypeIds, newZoneTypeId);
                    }
                }

                if (csz == cez)
                {
                    CheckChangeTerrain(map, cmx, csz, skipZoneTypeIds, newZoneTypeId);
                }
                else
                {
                    for (int zz = csz; zz != cez; zz += Math.Sign(cez - csz))
                    {
                        CheckChangeTerrain(map, cmx, zz, skipZoneTypeIds, newZoneTypeId);
                    }
                }
            }
        }

        public virtual void AddPathsBetweenPoints(CrawlerMap map, List<SampledPoint> locs, int mapEdgeSize,
            List<long> skipZoneTypeIds, long zoneTypeId, IRandom rand)
        {
            List<Point2I> remainingPoints = new List<Point2I>(locs);

            List<ConnectPointData> cityPoints = new List<ConnectPointData>();

            int centerId = 0;
            foreach (Point2I loc in locs)
            {
                ConnectPointData connectionData = new ConnectPointData()
                {
                    Id = ++centerId,
                    X = loc.X,
                    Z = loc.Z,
                    Data = loc,
                    MaxConnections = 3,
                };
                cityPoints.Add(connectionData);
            }

            List<ConnectedPairData> roadsToMake = _lineGenService.ConnectPoints(cityPoints, rand, 0.0f);

            foreach (ConnectedPairData pairData in roadsToMake)
            {
                ConnectPairOfPoints(map, pairData, mapEdgeSize, skipZoneTypeIds, zoneTypeId, rand);
            }
        }

        public void RemoveEdgePoints(CrawlerMap map, int edgeDistance)
        {

            for (int x = 0; x < map.Width; x++)
            {

                for (int z = 0; z < map.Height; z++)
                {
                    if (x < edgeDistance || x >= map.Width - 1 - edgeDistance || z < edgeDistance || z >= map.Height - 1 - edgeDistance)
                    {
                        map.Set(x, z, CellIndex.Terrain, 0);
                    }
                }
            }
        }

        public void SetDungeonEntranceAndExitPoints(CrawlerMap map, DungeonLevelGenArgs levelArgs)
        {


            List<Point2I> openPoints = new List<Point2I>();

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    if (map.Get(x, z, CellIndex.Terrain) > 0)
                    {
                        openPoints.Add(new Point2I(x, z));
                    }
                }
            }


            if (map.IsOutdoorDungeon())
            {

                int minx = openPoints.Min(p => p.X);
                int maxx = openPoints.Max(p => p.X);
                int minz = openPoints.Min(p => p.Z);
                int maxz = openPoints.Max(p => p.Z);

                List<Point2I> edgePoints = openPoints.Where(p => p.X == minx || p.Z == minz || p.X == maxx || p.Z == maxz).ToList();

                edgePoints = edgePoints.Where(p => p.X > 0 && p.X < map.Width - 1 && p.Z > 0 && p.Z < map.Height - 1).ToList();

                if (edgePoints.Count > 1)
                {
                    // If minx or maxx or minz or maxz are bad, there should be no points in here with
                    // those values so it should be safe to move the point toward the outer edges a bit.
                    foreach (Point2I pt in edgePoints)
                    {
                        if (pt.X == minx)
                        {
                            pt.X--;
                        }
                        else if (pt.X == maxx)
                        {
                            pt.X++;
                        }
                        else if (pt.Z == minz)
                        {
                            pt.Z--;
                        }
                        else if (pt.Z == maxz)
                        {
                            pt.Z++;
                        }
                    }

                    openPoints = edgePoints;
                }
            }


            List<Point2I> entrances = new List<Point2I>();

            Point2I entrance = openPoints[levelArgs.Rand.Next() % openPoints.Count];
            entrances.Add(entrance);
            openPoints.Remove(entrance);


            List<Point2I> farPoints = openPoints.Where(p => Math.Abs(p.X - entrance.X) + Math.Abs(p.Z - entrance.Z) >= 10).ToList();

            if (farPoints.Count > 0)
            {
                openPoints = farPoints;
            }

            Point2I exit = openPoints[levelArgs.Rand.Next() % openPoints.Count];
            entrances.Add(exit);

            foreach (Point2I pt in entrances)
            {
                map.Set(pt.X, pt.Z, CellIndex.Terrain, map.IsOutdoorDungeon() ? ZoneTypes.Road : map.ZoneTypeId);
            }
            levelArgs.EnterX = entrance.X;
            levelArgs.EnterZ = entrance.Z;
            levelArgs.ExitX = exit.X;
            levelArgs.ExitZ = exit.Z;

            if (map.IsOutdoorDungeon())
            {

                foreach (Point2I pt in entrances)
                {
                    SampledPoint closestCenter = null;
                    double closestDist = 100000;
                    foreach (SampledPoint center in levelArgs.RoomCenters)
                    {
                        float dx = center.X - pt.X;
                        float dz = center.Z - pt.Z;

                        float dist = dx * dx + dz * dz;

                        if (closestCenter == null || dist < closestDist)
                        {
                            closestCenter = center;
                            closestDist = dist;
                        }
                    }

                    if (closestCenter != null)
                    {
                        ConnectPairOfPoints(map, new ConnectedPairData()
                        {
                            Point1 = new ConnectPointData() { X = pt.X, Z = pt.Z },
                            Point2 = new ConnectPointData() { X = closestCenter.X, Z = closestCenter.Z },
                        }, 1, new List<long>(), ZoneTypes.Road, levelArgs.Rand);
                    }
                }
            }
        }

        public void AddRoomWithDoor(DungeonLevelGenArgs args, int x, int z, EMapDirs doorDir, long zoneTypeId)
        {
            Dictionary<EMapDirs, MapDir> dict = MapDirUtils.GetDirs();

            foreach (MapDir md in dict.Values)
            {
                int wallVal = md.Dir == doorDir ? WallTypes.Door : WallTypes.Wall;
                args.Map.SetDirWallBits(x, z, md.Dir, wallVal);
                if (zoneTypeId > 0)
                {
                    args.Map.Set(x, z, CellIndex.Terrain, zoneTypeId);
                }
                args.SetFlag(x, z, DungeonLevelFlags.RoomWithDoor);
            }
        }


        public void SetWallBitsFromDeltas(CrawlerMap map, int x, int z, int dx, int dz, int wallType)
        {
            MapDir dir = MapDirUtils.GetDirFromDeltas(dx, dz);

            map.SetDirWallBits(x, z, dir.Dir, wallType);
        }

        public void RemoveDisconnectedComponents(CrawlerMap map)
        {
            bool[,] validTerrain = new bool[map.Width, map.Height];

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    validTerrain[x, z] = map.Get(x, z, CellIndex.Terrain) > 0;
                }
            }

            int[,] componentIds = _lineGenService.GetConnectedComponents(map);

            Dictionary<int, int> counts = new Dictionary<int, int>();

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    if (componentIds[x, z] > 0)
                    {
                        if (!counts.ContainsKey(componentIds[x, z]))
                        {
                            counts[componentIds[x, z]] = 0;
                        }
                        counts[componentIds[x, z]]++;
                    }
                }
            }

            int maxQuantityComponentId = -1;
            int maxCount = -1;
            foreach (int id in counts.Keys)
            {
                if (counts[id] > maxCount)
                {
                    maxCount = counts[id];
                    maxQuantityComponentId = id;
                }
            }

            while (true)
            {
                bool didReconnectAnyId = false;

                foreach (int currComponentId in counts.Keys)
                {
                    if (currComponentId == maxQuantityComponentId)
                    {
                        continue;
                    }

                    bool didReconnectCurrentId = false;
                    for (int x = 0; x < map.Width; x++)
                    {
                        if (didReconnectCurrentId)
                        {
                            break;
                        }
                        for (int z = 0; z < map.Height; z++)
                        {
                            if (componentIds[x, z] == currComponentId)
                            {
                                if (x > 0 && componentIds[x - 1, z] == maxQuantityComponentId)
                                {
                                    didReconnectCurrentId = true;
                                    map.Set(x - 1, z, CellIndex.Walls, map.NorthWall(x - 1, z));
                                    break;
                                }
                                else if (x < map.Width - 1 && componentIds[x + 1, z] == maxQuantityComponentId)
                                {
                                    didReconnectCurrentId = true;
                                    map.Set(x, z, CellIndex.Walls, map.NorthWall(x, z));
                                }
                                else if (z > 0 && componentIds[x, z - 1] == maxQuantityComponentId)
                                {
                                    didReconnectCurrentId = true;
                                    map.Set(x, z - 1, CellIndex.Walls, map.EastWall(x - 1, z));
                                }
                                else if (z < map.Height && componentIds[x, z + 1] == maxQuantityComponentId)
                                {
                                    didReconnectCurrentId = true;
                                    map.Set(x, z, CellIndex.Walls, map.EastWall(x, z));
                                }
                            }
                        }
                    }

                    if (didReconnectCurrentId)
                    {
                        didReconnectAnyId = true;

                        for (int x = 0; x < map.Width; x++)
                        {
                            for (int z = 0; z < map.Height; z++)
                            {
                                if (componentIds[x, z] == currComponentId)
                                {
                                    componentIds[x, z] = maxQuantityComponentId;
                                }
                            }
                        }
                    }
                }


                for (int x = 0; x < map.Width; x++)
                {
                    for (int z = 0; z < map.Height; z++)
                    {
                        if (componentIds[x, z] != maxQuantityComponentId)
                        {
                            if (map.Details.Any(d => d.X == x && d.Z == z))
                            {
                                int compId = componentIds[x, z];
                                int compCount = counts[compId];
                                _logService.Info("Removing cell with detail? " + compId + " " + maxQuantityComponentId + " " + compCount + " " + counts.Keys.Count());
                            }
                            map.Set(x, z, CellIndex.Terrain, 0);
                        }
                    }
                }

                if (!didReconnectAnyId)
                {
                    break;
                }
            }
        }

        enum EModCellTypes
        {
            None = 0,
            SmallRoom = 1,
            RemoveCell = 2,
        };

        public void AddSmallRoomsAndBlankSpaces(DungeonLevelGenArgs levelArgs)
        {
            RoomGenSettings settings = _gameData.Get<RoomGenSettings>(_gs.ch);

            Dictionary<EMapDirs, MapDir> allMapDirs = MapDirUtils.GetDirs();

            bool[,] openCells = new bool[3, 3];

            for (int x = 1; x < levelArgs.Map.Width - 1; x++)
            {
                for (int z = 1; z < levelArgs.Map.Height - 1; z++)
                {
                    if (!levelArgs.Map.IsValidEmptyCell(x, z))
                    {
                        continue;
                    }

                    for (int xx = 0; xx < 3; xx++)
                    {
                        for (int zz = 0; zz < 3; zz++)
                        {
                            openCells[xx, zz] = true;
                        }
                    }

                    int openCellCount = 0;
                    int blockedCellCount = 0;

                    EModCellTypes modType = EModCellTypes.None;
                    if (levelArgs.Rand.NextDouble() < settings.SmallRoomChance)
                    {
                        modType = EModCellTypes.SmallRoom;
                    }
                    else if (levelArgs.Rand.NextDouble() < settings.RemoveCellChance)
                    {
                        modType = EModCellTypes.RemoveCell;
                    }
                    else
                    {
                        continue;
                    }
                    bool doNotAllowBlock = false;

                    int adjacentOpenCount = 0;

                    for (int xx = x - 1; xx <= x + 1; xx++)
                    {
                        for (int zz = z - 1; zz <= z + 1; zz++)
                        {

                            if (levelArgs.Map.Get(xx, zz, CellIndex.Terrain) == 0 ||
                                levelArgs.Map.Get(xx, zz, CellIndex.Walls) != 0)
                            {
                                openCells[xx - (x - 1), zz - (z - 1)] = false;
                                blockedCellCount++;

                                if (levelArgs.HasFlag(xx, zz, DungeonLevelFlags.RoomWithDoor) ||
                                    levelArgs.HasFlag(xx, zz, DungeonLevelFlags.RemovedBlock))
                                {
                                    doNotAllowBlock = true;
                                }
                            }
                            else
                            {
                                openCellCount++;
                                if (Math.Abs(xx - x) + Math.Abs(zz - z) == 1)
                                {
                                    adjacentOpenCount++;
                                }
                            }
                        }
                    }

                    if (!openCells[1, 1] || doNotAllowBlock)
                    {
                        continue;
                    }

                    if (modType == EModCellTypes.SmallRoom)
                    {

                        List<EMapDirs> dirChoices = new List<EMapDirs>();

                        foreach (MapDir md in allMapDirs.Values)
                        {
                            if (openCells[md.DX + 1, md.DZ + 1])
                            {
                                dirChoices.Add(md.Dir);
                            }
                        }

                        if (dirChoices.Count > 0)
                        {
                            EMapDirs chosenDir = dirChoices[levelArgs.Rand.Next() % dirChoices.Count];
                            AddRoomWithDoor(levelArgs, x, z, chosenDir, levelArgs.Map.ZoneTypeId);
                        }
                    }
                    else if (modType == EModCellTypes.RemoveCell && openCellCount >= 4)
                    {

                        bool blockOfFourIsOpen = false;
                        for (int nx = 0; nx <= 1; nx++)
                        {
                            if (blockOfFourIsOpen)
                            {
                                break;
                            }
                            for (int nz = 0; nz <= 1; nz++)
                            {
                                if (blockOfFourIsOpen)
                                {
                                    break;
                                }

                                bool currentFourAllOpen = true;
                                for (int xx = nx; xx <= nx + 1; xx++)
                                {
                                    for (int zz = nz; zz <= nz + 1; zz++)
                                    {
                                        if (!openCells[xx, zz])
                                        {
                                            currentFourAllOpen = false;
                                            break;
                                        }
                                    }
                                }
                                if (currentFourAllOpen)
                                {
                                    blockOfFourIsOpen = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }


        public void AddOutdoorDungeonZoneEdges(CrawlerMap map)
        {
            if (!map.IsOutdoorDungeon())
            {
                return;
            }

            int edgeSize = 2;
            foreach (MapCellDetail detail in map.Details)
            {
                if (detail.EntityTypeId != EntityTypes.Map)
                {
                    continue;
                }

                int dx = 0;
                int dz = 0;
                int x = detail.X;
                int z = detail.Z;
                if (detail.X <= edgeSize)
                {
                    dx = -1;
                    x = detail.X - 1;
                }
                else if (detail.X >= map.Width - 1 - edgeSize)
                {
                    dx = 1;
                    x = detail.X + 1;
                }
                else if (detail.Z <= edgeSize)
                {
                    dz = -1;
                    z = detail.Z - 1;
                }
                else if (detail.Z >= map.Height - 1 - edgeSize)
                {
                    dz = 1;
                    z = detail.Z + 1;
                }
                else
                {
                    _logService.Info("Bad Detail? " + detail.X + " " + detail.Z + " " + map.Width + " " + map.Height);
                }

                if (Math.Abs(dx) + Math.Abs(dz) == 1)
                {
                    map.EdgePoints.Add(new ZoneEdge()
                    {
                        X = x,
                        Z = z,
                        DX = dx,
                        DZ = dz,
                        ZoneTypeId = map.ZoneTypeId,
                    });

                    int cx = x;
                    int cz = z;

                    while (cx >= 0 && cz >= 0 && cx < map.Width && cz < map.Height)
                    {
                        long entityTypeId = map.Get(cx, cz, CellIndex.EntityType);
                        if (entityTypeId == 0 || entityTypeId == EntityTypes.Prop)
                        {
                            map.SetEntity(cx, cz, EntityTypes.RoomEdge, 1);
                        }
                        cx += dx;
                        cz += dz;
                    }
                }
            }
        }
    }
}



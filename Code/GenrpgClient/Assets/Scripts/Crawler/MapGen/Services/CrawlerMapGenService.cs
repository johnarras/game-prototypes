using Assets.Scripts.Crawler.MapGen.Helpers;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
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
using UnityEngine;

namespace Assets.Scripts.Crawler.MapGen.Services
{

    public interface ICrawlerMapGenService : IInitializable
    {
        ICrawlerMapGenHelper GetGenHelper(long mapType);
        Task<CrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData, CancellationToken token);
        void OneWayLink(CrawlerWorld world, long fromMapId, int fromX, int fromZ, long toMapId, int toX, int toZ);
        Task<CrawlerMap> GenerateRoguelikeDungeonLevel(PartyData party, CrawlerWorld world, long mapId, int enterX, int enterZ, CancellationToken token);
        bool RoomAreaIsBlank(int[,] roomIds, int minx, int maxx, int minz, int maxz);

        void AddMapBoundaryWalls(CrawlerMap map);
        void AddBoundaryWallsAtPoint(CrawlerMap map, int x, int z);

        void AddPathsBetweenPoints(CrawlerMap map, List<SampledPoint> locs, int mapEdgeSize, List<long> skipZoneTypeIds, long newZoneTypeId, IRandom rand);

        void ConnectPairOfPoints(CrawlerMap map, ConnectedPairData pairData, int mapEdgeSize,
            List<long> skipZoneTypeIds, long newZoneTypeId,
            IRandom rand);

        void SetEntranceAndExitPoints(CrawlerMap map, DungeonLevelGenArgs levelArgs);

        void AddRoomWithDoor(DungeonLevelGenArgs levelArgs, int x, int z, EMapDirs doorDir, long zoneTypeId);

        void RemoveDisconnectedComponents(CrawlerMap map);

        void AddSmallRoomsAndBlankSpaces(DungeonLevelGenArgs levelArgs);

        void SetWallBitsFromDeltas(CrawlerMap map, int x, int z, int dx, int dz, int wallType);
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

            if (genData.ZoneType == null)
            {
                if (genData.GenType != null && genData.GenType.WeightedZones.Count > 0)
                {
                    long zoneTypeId = RandUtils.GetRandomElement(genData.GenType.WeightedZones, rand).ZoneTypeId;

                    genData.ZoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zoneTypeId);

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
                else
                {
                    return null;
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

            ICrawlerMapGenHelper helper = GetGenHelper(genData.MapTypeId);
            NewCrawlerMap newMap = await helper.Generate(party, world, genData, token);

            if (newMap == null || newMap.Map == null)
            {
                _logService.Info("NullMap? " + genData.MapTypeId);
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
                CurrFloor = mapId - 1,
                MaxFloor = mapId,
                Level = mapId - 1,
                LevelDelta = 0,
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

        public void AddBoundaryWallsAtPoint(CrawlerMap map, int x, int z)
        {

            CheckCellDir(map, x, z, x + 1, z);
            CheckCellDir(map, x, z, x - 1, z);
            CheckCellDir(map, x, z, x, z - 1);
            CheckCellDir(map, x, z, x, z + 1);
        }

        private void CheckCellDir(CrawlerMap map, int x, int z, int nx, int nz)
        {
            bool looping = map.HasFlag(CrawlerMapFlags.IsLooping);

            int dx = nx - x;
            int dz = nz - z;

            if (dx != 0)
            {
                bool placeWall = false;
                if (nx < 0)
                {
                    if (!looping)
                    {
                        placeWall = true;
                    }
                    else
                    {
                        nx = map.Width - 1;
                    }
                }
                if (nx >= map.Width)
                {
                    if (!looping)
                    {
                        placeWall = true;
                    }
                    else
                    {
                        nx = 0;
                    }
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
                    if (!looping)
                    {
                        placeWall = true;
                    }
                    else
                    {
                        nz = map.Height - 1;
                    }
                }
                if (nz >= map.Height)
                {
                    if (!looping)
                    {
                        placeWall = true;
                    }
                    else
                    {
                        nz = 0;
                    }
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

        public void SetEntranceAndExitPoints(CrawlerMap map, DungeonLevelGenArgs levelArgs)
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



            Point2I entrance = openPoints[levelArgs.Rand.Next() % openPoints.Count];
            openPoints.Remove(entrance);
            Point2I exit = openPoints[levelArgs.Rand.Next() % openPoints.Count];

            levelArgs.EnterX = entrance.X;
            levelArgs.EnterZ = entrance.Z;
            levelArgs.ExitX = exit.X;
            levelArgs.ExitZ = exit.Z;

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

            List<EMapDirs> allMapDirs = MapDirUtils.GetDirs().Keys.ToList();

            bool[,] openCells = new bool[3, 3];

            for (int x = 1; x < levelArgs.Map.Width - 1; x++)
            {
                for (int z = 1; z < levelArgs.Map.Height - 1; z++)
                {
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
                    bool allCellsOpen = true;
                    for (int xx = x - 1; xx <= x + 1; xx++)
                    {
                        for (int zz = z - 1; zz <= z + 1; zz++)
                        {

                            if (levelArgs.Map.Get(xx, zz, CellIndex.Terrain) == 0 ||
                                levelArgs.Map.Get(xx, zz, CellIndex.Walls) != 0)
                            {
                                openCells[xx - (x - 1), zz - (z - 1)] = false;
                                allCellsOpen = false;
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
                            }
                        }
                    }

                    EMapDirs chosenDir = allMapDirs[levelArgs.Rand.Next() % allMapDirs.Count];

                    if (!doNotAllowBlock && !allCellsOpen && openCells[1, 1] && openCellCount == 4 && blockedCellCount == 5)
                    {
                        List<EMapDirs> dirChoices = new List<EMapDirs>();

                        if (openCells[0, 1])
                        {
                            if (openCells[0, 2] && openCells[1, 2])
                            {
                                dirChoices = new List<EMapDirs>() { EMapDirs.West, EMapDirs.North };
                            }
                            else if (openCells[0, 0] && openCells[1, 0])
                            {
                                dirChoices = new List<EMapDirs>() { EMapDirs.West, EMapDirs.South };
                            }
                        }
                        else if (openCells[2, 1])
                        {
                            if (openCells[2, 2] && openCells[1, 2])
                            {
                                dirChoices = new List<EMapDirs>() { EMapDirs.East, EMapDirs.North };
                            }
                            else if (openCells[1, 0] && openCells[2, 0])
                            {
                                dirChoices = new List<EMapDirs>() { EMapDirs.East, EMapDirs.South };
                            }
                        }

                        if (dirChoices.Count > 0)
                        {
                            chosenDir = dirChoices[levelArgs.Rand.Next() % dirChoices.Count];
                            allCellsOpen = true;
                        }
                    }

                    if (allCellsOpen)
                    {
                        if (modType == EModCellTypes.SmallRoom)
                        {
                            AddRoomWithDoor(levelArgs, x, z, chosenDir, ZoneTypes.Desert);
                        }
                        else if (modType == EModCellTypes.RemoveCell)
                        {
                            levelArgs.Map.Set(x, z, CellIndex.Terrain, 0);
                            levelArgs.SetFlag(x, z, DungeonLevelFlags.RemovedBlock);
                        }
                    }
                }
            }
        }
    }
}



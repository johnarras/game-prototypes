using Assets.Scripts.Crawler.MapGen.DungeonGen.Helpers;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.MapGen.Constants;
using OxDb.SharedGame.Crawler.MapGen.Entities;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Maps.Settings;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.MapGen.Helpers
{

    public class DungeonLevelFlags
    {
        public const long RoomWithDoor = 0;
        public const long RemovedBlock = 1;
        public const long EdgeCell = 2;
        public const long EdgeEndCell = 3;
    }


    public class DungeonLevelGenArgs
    {
        public IRandom Rand { get; set; }
        public CrawlerMap Map { get; set; }
        public int EnterX { get; set; }
        public int EnterZ { get; set; }
        public int ExitX { get; set; }
        public int ExitZ { get; set; }
        public int[,] RoomIds { get; set; }
        public int[,] AdjacentRoomIds { get; set; }
        public int[,] Flags { get; set; }

        public SmallIndexBitList OverlappedRoomIds { get; set; } = new SmallIndexBitList();

        public bool HasFlag(int x, int z, long flagIndex)
        {
            return FlagUtils.HasBitIndex(Flags[x, z], flagIndex);
        }
        public void SetFlag(int x, int z, long flagIndex)
        {
            Flags[x, z] |= (int)(1 << (int)flagIndex);
        }

        public void RemoveFlag(int x, int z, long flagIndex)
        {
            Flags[x, z] &= ~(1 << (int)flagIndex);
        }
    }

    public class DungeonMapGenHelper : BaseCrawlerMapGenHelper
    {
        public override long HelperKey => CrawlerMapTypes.Dungeon;


        int complexMapSize = 36;
        SetupDictionaryContainer<long, IDungeonGenHelper> _dungeonGenHelpers = new SetupDictionaryContainer<long, IDungeonGenHelper>();

        public override async Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData, CancellationToken token)
        {
            DungeonLevelGenArgs levelArgs = new DungeonLevelGenArgs()
            {
                Rand = new MyRandom(genData.World.Seed / 3 + genData.World.GetMaxMapId() * 19 + genData.CurrFloor),
            };

            CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

            CrawlerMapGenType genType = genData.GenType;

            genData.Looping = levelArgs.Rand.NextDouble() < genType.LoopingChance ? true : false;
            genData.DungeonTypeId = levelArgs.Rand.NextDouble() < genType.RandomWallsChance ? DungeonTypes.RandomWalls : DungeonTypes.BasicRooms;
            if (genData.DungeonTypeId != DungeonTypes.RandomWalls)
            {
                genData.Looping = false;
            }
            int width = RandUtils.IntRange(genType.MinWidth, genType.MaxWidth, levelArgs.Rand);
            int height = RandUtils.IntRange(genType.MinHeight, genType.MaxHeight, levelArgs.Rand);

            genData.DungeonTypeId = DungeonTypes.ComplexRooms;
            genData.Looping = false;
            width = complexMapSize;
            height = complexMapSize;
            if (genData.DungeonTypeId != DungeonTypes.RandomWalls)
            {
                width = (int)(width * mapSettings.CorridorDungeonSizeScale);
                height = (int)(height * mapSettings.CorridorDungeonSizeScale);

                if (genType.IsIndoors && genType.NextLevelIsDown)
                {
                    width = complexMapSize;
                    height = complexMapSize;
                    genData.DungeonTypeId = DungeonTypes.ComplexRooms;
                }
            }

            if (genData.MaxFloor == 0 || genData.PrevMap == null)
            {
                genData.MaxFloor = RandUtils.IntRange(genType.MinFloors, genType.MaxFloors, levelArgs.Rand);

                if (genData.CurrFloor == 0)
                {
                    genData.CurrFloor = 1;
                }
                if (levelArgs.Rand.NextDouble() < 0.2f && genType.MaxFloors > 1)
                {
                    genData.MaxFloor++;
                }

                levelArgs.Map = _worldService.CreateMap(genData, (int)width, (int)height);
                genData.Name = _zoneGenService.GenerateZoneName(genData.ZoneType.IdKey, levelArgs.Rand.Next(), false);
                if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld))
                {
                    if (!string.IsNullOrEmpty(party.RoguelikeDungeonName))
                    {
                        genData.Name = party.RoguelikeDungeonName;
                    }
                    else
                    {
                        party.RoguelikeDungeonName = genData.Name;
                    }
                }
            }
            else
            {
                genData.Level++;
                genData.CurrFloor++;
                levelArgs.Map = _worldService.CreateMap(genData, (int)width, (int)height);
            }

            genData.PrevMap = levelArgs.Map;

            levelArgs.Map.Name = genData.Name;

            levelArgs.ExitX = -1;
            levelArgs.ExitZ = -1;
            levelArgs.EnterX = -1;
            levelArgs.EnterZ = -1;

            levelArgs.RoomIds = new int[levelArgs.Map.Width, levelArgs.Map.Height];
            levelArgs.AdjacentRoomIds = new int[levelArgs.Map.Width, levelArgs.Map.Height];
            levelArgs.Flags = new int[levelArgs.Map.Width, levelArgs.Map.Height];

            if (_dungeonGenHelpers.TryGetValue(genData.DungeonTypeId, out IDungeonGenHelper helper))
            {
                if (!await helper.GenerateLevel(genData, levelArgs))
                {
                    return null;
                }
            }

            RemoveBlankMapEdges(levelArgs);

            _mapGenService.AddMapBoundaryWalls(levelArgs.Map);

            List<Point2I> entranceExitPoints = new List<Point2I>();
            entranceExitPoints.Add(new Point2I(levelArgs.EnterX, levelArgs.EnterZ));
            entranceExitPoints.Add(new Point2I(levelArgs.ExitX, levelArgs.ExitZ));

            MarkTilesNearEntrances(party, genData, levelArgs.Map, entranceExitPoints);

            if (_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                if (genData.CurrFloor < genData.MaxFloor)
                {
                    long currMapId = genData.FromMapId;
                    int currFromX = genData.FromMapX;
                    int currFromZ = genData.FromMapZ;

                    genData.FromMapId = levelArgs.Map.IdKey;
                    genData.FromMapX = levelArgs.ExitX;
                    genData.FromMapZ = levelArgs.ExitZ;

                    await _mapGenService.Generate(party, world, genData, token);

                    genData.FromMapId = currMapId;
                    genData.FromMapX = currFromX;
                    genData.FromMapZ = currFromZ;
                }
            }
            else
            {
                levelArgs.Map.Details.Add(new MapCellDetail() { EntityTypeId = EntityTypes.Map, EntityId = levelArgs.Map.IdKey + 1, X = levelArgs.ExitX, Z = levelArgs.ExitZ, ToX = -1, ToZ = -1, });
                levelArgs.Map.Details.Add(new MapCellDetail() { EntityTypeId = EntityTypes.Map, EntityId = levelArgs.Map.IdKey - 1, X = levelArgs.EnterX, Z = levelArgs.EnterZ, ToX = -1, ToZ = -1, });
            }

            List<Point2I> validEmptyCells = new List<Point2I>();

            List<Point2I> cornerCells = new List<Point2I>();
            for (int x = 0; x < levelArgs.Map.Width; x++)
            {
                for (int z = 0; z < levelArgs.Map.Height; z++)
                {
                    if (levelArgs.Map.IsValidEmptyCell(x, z))
                    {
                        validEmptyCells.Add(new Point2I(x, z));
                    }
                }
            }

            AddEncounters(party, genData, levelArgs.Map, validEmptyCells, levelArgs.Rand);

            AddSmallRoomAndCornerEncounters(party, genData, levelArgs, validEmptyCells, levelArgs.Rand);

            AddQuestItemLocations(party, genData, levelArgs.Map, validEmptyCells, levelArgs.Rand);

            AddMagicLocations(party, genData, levelArgs.Map, validEmptyCells, levelArgs.Rand);

            AddTeleportSquares(party, genData, levelArgs.Map, validEmptyCells, levelArgs.Rand);

            ModifyZoneTypes(party, genData, levelArgs.Map, levelArgs.RoomIds, levelArgs.Rand);

            AddRoomDoors(party, genData, levelArgs.Map, levelArgs.RoomIds, levelArgs.Rand);

            AddLevelMap(party, genData, levelArgs.Map, validEmptyCells, levelArgs.Rand);


            return new NewCrawlerMap() { Map = levelArgs.Map, EnterX = levelArgs.EnterX, EnterZ = levelArgs.EnterZ };
        }

        protected void AddMagicLocations(PartyData party, CrawlerMapGenData genData, CrawlerMap map, List<Point2I> validEmptyCells, IRandom rand)
        {
            List<Point2I> removeList = new List<Point2I>();
            IReadOnlyList<MapMagicType> mapMagics = _gameData.Get<MapMagicSettings>(_gs.ch).GetData();
            double specialTilechance = genData.GenType.SpecialTileChance;
            foreach (Point2I pt in validEmptyCells)
            {
                if (rand.NextDouble() > specialTilechance)
                {
                    continue;
                }

                foreach (MapMagicType mtype in mapMagics)
                {
                    if (map.Level < mtype.MinLevel || rand.NextDouble() > mtype.Weight)
                    {
                        continue;
                    }

                    int xmin = pt.X;
                    int xmax = pt.X;
                    int zmin = pt.Z;
                    int zmax = pt.Z;

                    while (xmin > 0 && rand.NextDouble() < mtype.SpreadChance)
                    {
                        xmin--;
                    }
                    while (xmax < map.Width - 1 && rand.NextDouble() < mtype.SpreadChance)
                    {
                        xmax++;
                    }
                    while (zmin > 0 && rand.NextDouble() < mtype.SpreadChance)
                    {
                        zmin--;
                    }
                    while (zmax < map.Height - 1 && rand.NextDouble() < mtype.SpreadChance)
                    {
                        zmax++;
                    }

                    for (int xx = xmin; xx <= xmax; xx++)
                    {
                        for (int zz = zmin; zz <= zmax; zz++)
                        {
                            if (map.Get(xx, zz, CellIndex.Terrain) < 1)
                            {
                                continue;
                            }

                            int bits = map.GetEntityId(xx, zz, EntityTypes.MapMagic);
                            if (FlagUtils.MatchesAnyBits(bits, (1 << MapMagics.Silence)) &&
                                FlagUtils.MatchesAnyBits(bits, (1 << MapMagics.NoMagic)))
                            {
                                continue;
                            }
                            bits |= (1 << (int)(mtype.IdKey - 1));

                            map.SetEntity(xx, zz, EntityTypes.MapMagic, bits);
                            Point2I magicPt = validEmptyCells.FirstOrDefault(p => p.X == xx && p.Z == zz);
                            if (!removeList.Contains(magicPt))
                            {
                                removeList.Add(magicPt);
                            }
                        }
                    }
                }
            }
            foreach (Point2I removePt in removeList)
            {
                validEmptyCells.Remove(removePt);
            }


        }

        protected void AddQuestItemLocations(PartyData party, CrawlerMapGenData genData, CrawlerMap map, List<Point2I> validEmptyCells, IRandom rand)
        {
            for (int i = 0; i < 3; i++)
            {
                if (validEmptyCells.Count < 1)
                {
                    break;
                }
                Point2I pt = validEmptyCells[rand.Next(validEmptyCells.Count)];
                validEmptyCells.Remove(pt);
                map.SetEntity(pt.X, pt.Z, EntityTypes.QuestItem, byte.MaxValue);
            }
        }

        protected void MarkTilesNearEntrances(PartyData party, CrawlerMapGenData genData, CrawlerMap map, List<Point2I> entranceExitPoints)
        {

            foreach (Point2I point in entranceExitPoints)
            {
                for (int xx = point.X - 1; xx <= point.X + 1; xx++)
                {
                    if (xx < 0 || xx >= map.Width)
                    {
                        continue;
                    }
                    for (int zz = point.Z - 1; zz <= point.Z + 1; zz++)
                    {
                        if (zz < 0 || zz >= map.Height)
                        {
                            continue;
                        }

                        map.SetEntity(xx, zz, EntityTypes.MapEncounter, MapEncounters.OtherFeature);
                    }
                }
            }
        }

        protected void AddEncounters(PartyData party, CrawlerMapGenData genData, CrawlerMap map, List<Point2I> validEmptyCells, IRandom rand)
        {
            MapEncounterSettings encounterSettings = _gameData.Get<MapEncounterSettings>(_gs.ch);

            int encountersToPlace = (int)(validEmptyCells.Count * encounterSettings.EncounterChance);

            int startEncountersToPlace = encountersToPlace;

            int encounterTries = encountersToPlace * 20;

            for (int i = 0; i < encounterTries && encountersToPlace > 0; i++)
            {
                if (validEmptyCells.Count < 1)
                {
                    continue;
                }

                long encounter = GetRandomEncounter(rand);

                if (encounter == MapEncounters.Treasure && !_optionsService.HasOption(party, CrawlerOptions.RandomChests))
                {
                    continue;
                }

                if (encounter == MapEncounters.Stats && !_optionsService.HasOption(party, CrawlerOptions.StatUpgradeObjects))
                {
                    continue;
                }

                Point2I pt = validEmptyCells[rand.Next() % validEmptyCells.Count];
                validEmptyCells.Remove(pt);

                map.SetEntity(pt.X, pt.Z, EntityTypes.MapEncounter, GetRandomEncounter(rand));
                encountersToPlace--;
            }

            if (!_optionsService.HasOption(party, CrawlerOptions.RandomMonsters))
            {
                for (int i = 0; i < startEncountersToPlace * 2; i++)
                {
                    if (validEmptyCells.Count < 1)
                    {
                        continue;
                    }

                    Point2I pt = validEmptyCells[rand.Next() % validEmptyCells.Count];
                    validEmptyCells.Remove(pt);
                    map.SetEntity(pt.X, pt.Z, EntityTypes.MapEncounter, MapEncounters.Monsters);
                }
            }

        }

        protected void AddSmallRoomAndCornerEncounters(PartyData party, CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs, List<Point2I> validEmptyCells, IRandom rand)
        {
            MapEncounterSettings encounterSettings = _gameData.Get<MapEncounterSettings>(_gs.ch);


            CrawlerMap map = levelArgs.Map;

            List<Point2I> cornerValidEmptyCells = new List<Point2I>();

            foreach (Point2I pt in validEmptyCells)
            {
                if (levelArgs.RoomIds[pt.X, pt.Z] != 0 &&
                    map.IsValidEmptyCell(pt.X, pt.Z) &&
                    (levelArgs.HasFlag(pt.X, pt.Z, DungeonLevelFlags.EdgeEndCell) ||
                    levelArgs.HasFlag(pt.X, pt.Z, DungeonLevelFlags.RoomWithDoor)))
                {
                    cornerValidEmptyCells.Add(pt);
                }
            }

            List<MapEncounterType> cornerTypes = encounterSettings.GetData().Where(x => x.IsCornerSmallRoomItem).ToList();


            foreach (Point2I pt in cornerValidEmptyCells)
            {
                if (levelArgs.Rand.NextDouble() > encounterSettings.CornerSmallRoomEncounterChance)
                {
                    continue;
                }

                MapEncounterType cornerType = RandUtils.GetRandomElement(cornerTypes, levelArgs.Rand);


                validEmptyCells.Remove(pt);


                map.SetEntity(pt.X, pt.Z, EntityTypes.MapEncounter, cornerType.IdKey);

            }
        }

        protected long GetRandomEncounter(IRandom rand)
        {

            MapEncounterType encounter = RandUtils.GetRandomElement(_gameData.Get<MapEncounterSettings>(_gs.ch).GetData(), rand);

            if (encounter != null)
            {
                return encounter.IdKey;
            }

            return 0;

        }
        const float extraLengthChance = 0.25f;
        protected int GetRoomDeltaSize(IRandom rand, int roomEdgeDist)
        {
            int retval = 1;

            for (int i = 0; i < 3; i++)
            {
                if (retval >= roomEdgeDist - 2)
                {
                    return retval;
                }
                if (rand.Next() < extraLengthChance)
                {
                    retval++;
                }
                else
                {
                    break;
                }
            }
            return retval;
        }


        private bool[] GetBlockedDirs(CrawlerMap map, Dictionary<EMapDirs, MapDir> mapDirs, int x, int z)
        {
            bool[] isBlocked = new bool[mapDirs.Values.Count];
            foreach (MapDir dir in mapDirs.Values)
            {
                int blockingBits = _crawlerMapService.GetBlockingBits(map, x, z, x + dir.DX, z + dir.DZ, false);

                isBlocked[dir.Index] = WallTypes.IsBlockingType(blockingBits);
            }
            return isBlocked;
        }

        private bool CanBeNearTeleportCell(CrawlerMap map, int x, int z)
        {
            int extraRadius = 1;
            for (int xx = x - extraRadius; xx <= x + extraRadius; xx++)
            {
                int nx = (xx + map.Width) % map.Width;
                for (int zz = z - extraRadius; zz <= z + extraRadius; zz++)
                {
                    int nz = (zz + map.Height) % map.Height;

                    if (!map.IsValidEmptyCell(nx, nz))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void AddTeleportSquares(PartyData party, CrawlerMapGenData genData, CrawlerMap map, List<Point2I> validEmptyCells, IRandom rand)
        {

            List<Point2I> teleportEntryPoints = new List<Point2I>();
            Dictionary<EMapDirs, MapDir> mapDirs = MapDirUtils.GetDirs();
            bool[,][] allBlockedDirs = new bool[map.Width, map.Height][];

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    allBlockedDirs[x, z] = GetBlockedDirs(map, mapDirs, x, z);
                }
            }

            for (int x = 1; x < map.Width - 1; x++)
            {
                for (int z = 1; z < map.Height - 1; z++)
                {
                    bool[] currBlocked = allBlockedDirs[x, z];

                    int blockCount = currBlocked.Count(x => x == true);

                    if (!CanBeNearTeleportCell(map, x, z))
                    {
                        continue;
                    }

                    bool canBeTeleport = true;

                    if (blockCount < 2)
                    {
                        for (int d = 0; d < currBlocked.Length; d++)
                        {
                            MapDir currDir = mapDirs[(EMapDirs)d];
                            if (!currBlocked[d])
                            {
                                int cx = (x + currDir.DX + map.Width) % map.Width;
                                int cz = (z + currDir.DZ + map.Height) % map.Height;
                                bool[] currBlockedBits = allBlockedDirs[cx, cz];

                                List<int> otherDirs = new List<int>()
                                {
                                    (d + currBlocked.Length - 1) % currBlocked.Length,
                                    (d + currBlocked.Length + 1) % currBlocked.Length,
                                };

                                int otherOkDirs = 0;
                                foreach (int otherDirIndex in otherDirs)
                                {
                                    if (!currBlocked[otherDirIndex])
                                    {
                                        MapDir prevDir = mapDirs[(EMapDirs)otherDirIndex];
                                        int px = (x + prevDir.DX + map.Width) % map.Width;
                                        int pz = (z + prevDir.DZ + map.Height) % map.Height;
                                        if (!CanBeNearTeleportCell(map, px, pz))
                                        {
                                            continue;
                                        }
                                        bool[] otherBlockedBits = allBlockedDirs[px, pz];
                                        if (otherBlockedBits[d])
                                        {
                                            continue;
                                        }
                                        otherOkDirs++;
                                    }
                                }

                                if (otherOkDirs < 1)
                                {
                                    canBeTeleport = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (canBeTeleport && !map.Details.FastAny(d => d.X == x && d.Z == z))
                    {
                        teleportEntryPoints.Add(new Point2I(x, z));
                    }
                }
            }

            int entryPointCount = teleportEntryPoints.Count;
            List<Point2I> teleportExitPoints = new List<Point2I>();

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    if (map.IsValidEmptyCell(x, z) &&
                        !teleportEntryPoints.FastAny(t => t.X == x && t.Z == z)
                        )
                    {
                        teleportExitPoints.Add(new Point2I((int)x, (int)z));
                    }
                }
            }

            int exitPointCount = teleportExitPoints.Count;

            CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

            int teleportQuantityLeft = mapSettings.MinTeleportQuantity;

            while (rand.NextDouble() < mapSettings.ExtraTeleportChance && teleportQuantityLeft < mapSettings.MaxTeleportQuantity)
            {
                teleportQuantityLeft++;
            }

            int teleportQuantity = 0;
            teleportEntryPoints = teleportEntryPoints.OrderBy(x => HashUtils.NewGuid()).ToList();

            List<List<Point2I>> allCheckLists = new List<List<Point2I>>() { teleportEntryPoints, teleportExitPoints };

            while (teleportQuantityLeft > 0 && teleportEntryPoints.Count > 0 && teleportExitPoints.Count > 0)
            {
                Point2I enterPoint = teleportEntryPoints[rand.Next() % teleportEntryPoints.Count];

                teleportEntryPoints.Remove(enterPoint);

                Point2I currPoint = validEmptyCells.FirstOrDefault(p => p.X == enterPoint.X && p.Z == enterPoint.Z);
                if (currPoint != null)
                {
                    validEmptyCells.Remove(currPoint);
                }


                List<Point2I> okPoints = new List<Point2I>();

                foreach (Point2I teleExitPoint in teleportExitPoints)
                {
                    int dx = enterPoint.X - teleExitPoint.X;
                    int dz = enterPoint.Z - teleExitPoint.Z;

                    while (dx < -map.Width / 2)
                    {
                        dx += map.Width;
                    }
                    while (dx > map.Width / 2)
                    {
                        dx -= map.Width;
                    }
                    while (dz < -map.Height / 2)
                    {
                        dz += map.Height;
                    }
                    while (dz > map.Height / 2)
                    {
                        dz -= map.Height;
                    }


                    if (Math.Abs(dx) + Math.Abs(dz) >= 3)
                    {
                        okPoints.Add(teleExitPoint);
                    }
                }


                if (okPoints.Count == 0)
                {
                    continue;
                }

                Point2I exitPt = okPoints[rand.Next() % okPoints.Count];

                map.Details.Add(new MapCellDetail()
                {
                    X = enterPoint.X,
                    Z = enterPoint.Z,
                    EntityTypeId = EntityTypes.TeleportIn,
                    ToX = exitPt.X,
                    ToZ = exitPt.Z,
                });

                teleportQuantityLeft--;
                teleportQuantity++;

                // Don't let any teleports be within one unit of the teleport entrances or exits.
                List<Point2I> badPoints = new List<Point2I>() { enterPoint, exitPt };

                foreach (Point2I bp in badPoints)
                {
                    foreach (List<Point2I> checkList in allCheckLists)
                    {
                        List<Point2I> removeList = new List<Point2I>();
                        foreach (Point2I op in checkList)
                        {
                            int dx = bp.X - op.X;
                            int dz = bp.Z - op.Z;

                            while (dx < -map.Width / 2)
                            {
                                dx += map.Width;
                            }
                            while (dx > map.Width / 2)
                            {
                                dx -= map.Width;
                            }
                            while (dz < -map.Height / 2)
                            {
                                dz += map.Height;
                            }
                            while (dz > map.Height / 2)
                            {
                                dz -= map.Height;
                            }

                            if (Math.Abs(dx) <= 1 && Math.Abs(dz) <= 1)
                            {
                                removeList.Add(op);
                            }
                        }

                        foreach (Point2I removeMe in removeList)
                        {
                            checkList.Remove(removeMe);
                        }
                    }
                }
            }
        }



        public override NpcQuestMaps GetQuestMapsForNpc(PartyData party, CrawlerWorld world, CrawlerMap map, MapCellDetail npcDetail, IRandom rand)
        {
            NpcQuestMaps maps = new NpcQuestMaps();

            if (map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon)
            {
                maps.PrimaryMaps.Add(new MapLink() { Map = map, Link = npcDetail });
            }

            return maps;
        }


        private void ModifyZoneTypes(PartyData party, CrawlerMapGenData genData, CrawlerMap map, int[,] roomIds, IRandom rand)
        {

            List<int> distinctRoomIds = new List<int>();
            for (int x = 0; x < roomIds.GetLength(0); x++)
            {
                for (int z = 0; z < roomIds.GetLength(1); z++)
                {
                    if (roomIds[x, z] > 0 && !distinctRoomIds.Contains(roomIds[x, z]))
                    {
                        distinctRoomIds.Add(roomIds[x, z]);
                    }
                }
            }

            List<long> zoneTypes = new List<long>();

            foreach (CrawlerMapGenType genType in genData.MapType.GenTypes)
            {
                foreach (WeightedZoneType wzt in genType.WeightedZones)
                {
                    if (!zoneTypes.Contains(wzt.ZoneTypeId) && wzt.ZoneTypeId != map.ZoneTypeId)
                    {
                        zoneTypes.Add(wzt.ZoneTypeId);
                    }
                }
            }

            if (zoneTypes.Count < 1)
            {
                return;
            }

            long replacementZoneTypeId = zoneTypes[rand.Next() % zoneTypes.Count];

            CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

            foreach (int roomId in distinctRoomIds)
            {
                if (zoneTypes.Count < 1)
                {
                    break;
                }
                if (rand.NextDouble() > genData.MapType.RoomIsDifferentZoneTypeChance)
                {
                    continue;
                }

                for (int x = 0; x < map.Width; x++)
                {
                    for (int z = 0; z < map.Height; z++)
                    {
                        if (roomIds[x, z] == roomId)
                        {
                            if (map.Get(x, z, CellIndex.Terrain) > 0)
                            {
                                map.Set(x, z, CellIndex.Terrain, replacementZoneTypeId);
                            }
                        }
                    }
                }
            }
        }

        // Try to add doors between rooms and non-rooms.
        private void AddRoomDoors(PartyData party, CrawlerMapGenData genData, CrawlerMap map, int[,] roomIds, IRandom rand)
        {
            if (genData.DungeonTypeId == DungeonTypes.RandomWalls)
            {
                return;
            }

            bool[,] hasNorthEntrance = new bool[map.Width, map.Height];
            bool[,] hasEastEntrance = new bool[map.Width, map.Height];
            bool[,] hasEntrance = new bool[map.Width, map.Height];
            bool[,] badEntrance = new bool[map.Width, map.Height];

            for (int x = 0; x < map.Width - 1; x++)
            {
                for (int z = 0; z < map.Height - 1; z++)
                {
                    int currTerrain = map.Get(x, z, CellIndex.Terrain);
                    if (currTerrain == 0)
                    {
                        continue;
                    }
                    int currRoomId = roomIds[x, z];

                    if (map.Get(x, z + 1, CellIndex.Terrain) > 0 && roomIds[x, z + 1] != currRoomId &&
                        _crawlerMapService.GetBlockingBits(map, x, z, x, z + 1, false) == WallTypes.None)
                    {
                        hasNorthEntrance[x, z] = true;
                        hasEntrance[x, z] = true;
                    }
                    if (map.Get(x, z, CellIndex.Terrain) > 0 && roomIds[x + 1, z] != currRoomId &&
                        _crawlerMapService.GetBlockingBits(map, x, z, x + 1, z, false) == WallTypes.None)
                    {
                        hasEastEntrance[x, z] = true;
                        hasEntrance[x, z] = true;
                    }
                }
            }

            for (int x = 1; x < map.Width - 1; x++)
            {
                for (int z = 1; z < map.Height - 1; z++)
                {
                    if (hasEntrance[x, z])
                    {
                        bool isNearbyEntrance = false;
                        for (int xx = x - 1; xx <= x + 1; xx++)
                        {
                            for (int zz = z - 1; zz <= z + 1; zz++)
                            {
                                if (xx == x && zz == z)
                                {
                                    continue;
                                }
                                if (hasEntrance[xx, zz])
                                {
                                    isNearbyEntrance = true;
                                }
                            }
                        }

                        if (isNearbyEntrance)
                        {
                            for (int xx = x - 1; xx <= x + 1; xx++)
                            {
                                for (int zz = z - 1; zz <= z + 1; zz++)
                                {
                                    badEntrance[xx, zz] = true;
                                }
                            }
                        }
                    }
                }
            }


            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    if (badEntrance[x, z])
                    {
                        continue;
                    }

                    if (hasNorthEntrance[x, z])
                    {
                        map.AddBits(x, z, CellIndex.Walls, (WallTypes.Door << MapWallBits.NWallStart));
                    }
                    if (hasEastEntrance[x, z])
                    {
                        map.AddBits(x, z, CellIndex.Walls, (WallTypes.Door << MapWallBits.EWallStart));
                    }
                }
            }
        }

        private void AddLevelMap(PartyData party, CrawlerMapGenData genData, CrawlerMap map, List<Point2I> validEmptyCells, IRandom rand)
        {

            if (validEmptyCells.Count < 1)
            {
                return;
            }

            Point2I pt = validEmptyCells[rand.Next() % validEmptyCells.Count];
            validEmptyCells.Remove(pt);

            map.SetEntity(pt.X, pt.Z, EntityTypes.MapEncounter, MapEncounters.LevelMap);

        }

        private void RemoveBlankMapEdges(DungeonLevelGenArgs args)
        {
            CrawlerMap map = args.Map;


            int minx = 10000;
            int maxx = -1;
            int minz = 10000;
            int maxz = -1;

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    if (map.Get(x, z, CellIndex.Terrain) > 0)
                    {
                        minx = Math.Min(x, minx);
                        maxx = Math.Max(x, maxx);
                        minz = Math.Min(z, minz);
                        maxz = Math.Max(z, maxz);
                    }
                }
            }

            if (minx != 0 ||
                minz != 0 ||
                maxx != map.Width - 1 ||
                maxz != map.Height - 1)
            {

                int newWidth = maxx - minx + 1;
                int newHeight = maxz - minz + 1;



                byte[] newData = new byte[newWidth * newHeight * CellIndex.Max];

                for (int x = minx; x <= maxx; x++)
                {
                    for (int z = minz; z <= maxz; z++)
                    {

                        for (int offset = 0; offset < CellIndex.Max; offset++)
                        {
                            int index = GetDataIndex(x - minx, z - minz, newWidth, newHeight, offset);
                            newData[index] = map.Get(x, z, offset);

                        }
                    }
                }

                map.Data = newData;
                map.Width = newWidth;
                map.Height = newHeight;
                args.EnterX -= minx;
                args.EnterZ -= minz;
                args.ExitX -= minx;
                args.ExitZ -= minz;
            }

            if (!map.HasFlag(CrawlerMapFlags.IsLooping))
            {
                for (int x = 0; x < map.Width; x++)
                {
                    if (map.Get(x, 0, CellIndex.Terrain) == 0 &&
                        map.Get(x, map.Height - 1, CellIndex.Terrain) == 0)
                    {
                        map.Set(x, map.Height - 1, CellIndex.Walls, map.EastWall(x, map.Height - 1));
                    }
                }

                for (int z = 0; z < map.Height; z++)
                {
                    if (map.Get(0, z, CellIndex.Terrain) == 0 &&
                        map.Get(map.Width - 1, z, CellIndex.Terrain) == 0)
                    {
                        map.Set(map.Width - 1, z, CellIndex.Walls, map.NorthWall(map.Width - 1, z));
                    }
                }
            }
        }

        private int GetDataIndex(int x, int z, int width, int height, int offset)
        {
            return offset * width * height + z * width + x;
        }
    }
}


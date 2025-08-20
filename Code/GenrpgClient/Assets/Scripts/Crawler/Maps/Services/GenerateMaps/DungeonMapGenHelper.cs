
using Genrpg.Shared.Crawler.MapGen.Entities;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.ProcGen.Services;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Services.GenerateMaps
{
    public class DungeonMapGenHelper : BaseCrawlerMapGenHelper
    {
        public override long Key => CrawlerMapTypes.Dungeon;

        private ISamplingService _samplingService = null;

        public override async Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData, CancellationToken token)
        {
            await Task.CompletedTask;
            IRandom rand = new MyRandom(genData.World.Seed / 3 + genData.World.MaxMapId * 19 + genData.CurrFloor);

            CrawlerMap map = null;

            CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

            CrawlerMapGenType genType = genData.GenType;

            if (genData.MaxFloor == 0 || genData.PrevMap == null)
            {
                genData.MaxFloor = MathUtils.IntRange(genType.MinFloors, genType.MaxFloors, rand);

                if (genData.CurrFloor == 0)
                {
                    genData.CurrFloor = 1;
                }
                if (rand.NextDouble() < 0.2f && genType.MaxFloors > 1)
                {
                    genData.MaxFloor++;
                }

                genData.Looping = rand.NextDouble() < genType.LoopingChance ? true : false;
                genData.RandomWallsDungeon = rand.NextDouble() < genType.RandomWallsChance ? true : false;
                if (!genData.RandomWallsDungeon)
                {
                    genData.Looping = false;
                }
                int width = MathUtils.IntRange(genType.MinWidth, genType.MaxWidth, rand);
                int height = MathUtils.IntRange(genType.MinHeight, genType.MaxHeight, rand);

                if (!genData.RandomWallsDungeon)
                {
                    width = (int)(width * mapSettings.CorridorDungeonSizeScale);
                    height = (int)(height * mapSettings.CorridorDungeonSizeScale);
                }

                map = _worldService.CreateMap(genData, (int)width, (int)height);
                genData.Name = _zoneGenService.GenerateZoneName(genData.ZoneType.IdKey, rand.Next(), false);


            }
            else
            {
                genData.Level++;
                genData.CurrFloor++;
                map = _worldService.CreateMap(genData, genData.PrevMap.Width, genData.PrevMap.Height);
            }

            genData.PrevMap = map;

            map.Name = genData.Name;

            int exitX = -1;
            int exitZ = -1;
            int enterX = -1;
            int enterZ = -1;

            int[,] roomIds = new int[map.Width, map.Height];

            if (genData.RandomWallsDungeon)
            {
                double wallChance = MathUtils.FloatRange(genType.MinWallChance, genType.MaxWallChance, rand);
                double doorChance = MathUtils.FloatRange(genType.MinDoorChance, genType.MaxDoorChance, rand);
                for (int x = 0; x < map.Width; x++)
                {
                    for (int z = 0; z < map.Height; z++)
                    {
                        map.Set(x, z, CellIndex.Terrain, genData.ZoneType.IdKey);
                        int index = map.GetIndex(x, z);
                        int wallValue = 0;
                        if (rand.NextDouble() < wallChance)
                        {
                            if (x == map.Width - 1 && !map.HasFlag(CrawlerMapFlags.IsLooping))
                            {
                                wallValue |= WallTypes.Wall << MapWallBits.EWallStart;
                            }
                            else if (rand.NextDouble() > doorChance)
                            {
                                wallValue |= WallTypes.Wall << MapWallBits.EWallStart;
                            }
                            else
                            {
                                wallValue |= WallTypes.Door << MapWallBits.EWallStart;
                            }

                            if (z == map.Height - 1 && !map.HasFlag(CrawlerMapFlags.IsLooping))
                            {
                                wallValue |= WallTypes.Wall << MapWallBits.NWallStart;
                            }
                            else if (rand.NextDouble() > doorChance)
                            {
                                wallValue |= (WallTypes.Wall << MapWallBits.NWallStart);
                            }
                            else
                            {
                                wallValue |= WallTypes.Door << MapWallBits.NWallStart;
                            }

                        }
                        else
                        {
                            if (x == map.Width - 1 && !map.HasFlag(CrawlerMapFlags.IsLooping))
                            {
                                wallValue |= WallTypes.Wall << MapWallBits.EWallStart;
                            }
                            if (z == map.Height - 1 && !map.HasFlag(CrawlerMapFlags.IsLooping))
                            {
                                wallValue |= WallTypes.Wall << MapWallBits.NWallStart;
                            }
                        }
                        map.Set(x, z, CellIndex.Walls, wallValue);
                    }
                }

                AddRandomRooms(party, genData, map, roomIds, rand);
                ConnectOpenCells(map, genData, rand);

                double roomTimes = map.Width * map.Height / 200.0f;

                double roomremainder = roomTimes - (int)roomTimes;
                roomTimes = (int)roomTimes;
                if (rand.NextDouble() < roomremainder)
                {
                    roomTimes++;
                }

                int maxRoomSize = 6;
                for (int r = 0; r < roomTimes; r++)
                {
                    int minx = MathUtils.IntRange(0, map.Width - maxRoomSize - 1, rand);
                    int maxx = minx + MathUtils.IntRange(maxRoomSize / 2, maxRoomSize, rand);

                    int minz = MathUtils.IntRange(0, map.Height - maxRoomSize - 1, rand);
                    int maxz = MathUtils.IntRange(maxRoomSize / 2, maxRoomSize, rand);

                    for (int x = minx; x < maxx; x++)
                    {
                        for (int z = minz; z < maxz; z++)
                        {
                            map.Set(x, z, CellIndex.Walls, 0);
                        }
                    }
                }


                int exitEdgeDistance = 1;
                enterX = MathUtils.IntRange(exitEdgeDistance, map.Width - 1 - exitEdgeDistance, rand);
                enterZ = MathUtils.IntRange(exitEdgeDistance, map.Height - 1 - exitEdgeDistance, rand);

                do
                {
                    exitX = MathUtils.IntRange(exitEdgeDistance, map.Width - 1 - exitEdgeDistance, rand);
                    exitZ = MathUtils.IntRange(exitEdgeDistance, map.Height - 1 - exitEdgeDistance, rand);
                }
                while (enterX == exitX && enterZ == exitZ);

                List<PointXZ> usedPoints = new List<PointXZ>();
                usedPoints.Add(new PointXZ(enterX, enterZ));
                usedPoints.Add(new PointXZ(exitX, exitZ));

                for (int i = 0; i < 3; i++)
                {
                    do
                    {
                        int px = rand.Next() % map.Width;
                        int pz = rand.Next() % map.Height;

                        bool matchedExistingPoint = false;

                        foreach (PointXZ pt in usedPoints)
                        {
                            if (pt.X == px && pt.Z == pz)
                            {
                                matchedExistingPoint = true;
                                break;
                            }
                        }

                        if (!matchedExistingPoint)
                        {
                            usedPoints.Add(new PointXZ(px, pz));
                            break;
                        }
                    }
                    while (true);

                }
            }
            else
            {
                // bool[,] clearCells = AddCorridors(map, genData, rand, MathUtils.FloatRange(genType.MinCorridorDensity, genType.MaxCorridorDensity, rand));

                // Add rooms first.

                int roomCount = (int)(Math.Sqrt(map.Width * map.Height) * MathUtils.FloatRange(genType.MinCorridorDensity, genType.MaxCorridorDensity, rand));

                int edgeSize = 1;
                SamplingData sd = new SamplingData()
                {
                    MaxAttemptsPerItem = 20,
                    Count = roomCount,
                    MinSeparation = 4,
                    XMin = edgeSize,
                    YMin = edgeSize,
                    XMax = map.Width - 1 - edgeSize,
                    YMax = map.Height - 1 - edgeSize,
                };

                List<PointXZ> roomCenters = _samplingService.PlanePoissonSampleInteger(sd);

                List<ConnectPointData> connectPoints = new List<ConnectPointData>();

                for (int p = 0; p < roomCenters.Count; p++)
                {
                    PointXZ pt = roomCenters[p];
                    connectPoints.Add(new ConnectPointData()
                    {
                        X = pt.X,
                        Z = pt.Z,
                        Id = p + 1,
                        MaxConnections = 4,
                        MinDistToOther = 1,
                    });
                }

                bool[,] clearCells = new bool[map.Width, map.Height];

                for (int i = 0; i < roomCenters.Count; i++)
                {
                    int minx = roomCenters[i].X;
                    int maxx = roomCenters[i].X;
                    int minz = roomCenters[i].Z;
                    int maxz = roomCenters[i].Z;

                    int width = MathUtils.IntRange(2, 3, rand) + MathUtils.IntRange(0, 2, rand);
                    int height = MathUtils.IntRange(2, 3, rand) + MathUtils.IntRange(0, 2, rand);
                    if (width == 2 && height == 2)
                    {
                        if (rand.NextDouble() < 0.5f)
                        {
                            width++;
                        }
                        else
                        {
                            height++;
                        }
                    }

                    while (maxx - minx + 1 < width)
                    {
                        if (rand.NextDouble() < 0.5f)
                        {
                            minx--;
                        }
                        else
                        {
                            maxx++;
                        }
                    }

                    while (maxz - minz + 1 < height)
                    {
                        if (rand.NextDouble() < 0.5f)
                        {
                            minz--;
                        }
                        else
                        {
                            maxz++;
                        }
                    }

                    minx = MathUtils.Clamp(edgeSize, minx, map.Width - edgeSize - 1);
                    maxx = MathUtils.Clamp(edgeSize, maxx, map.Width - edgeSize - 1);
                    minz = MathUtils.Clamp(edgeSize, minz, map.Height - edgeSize - 1);
                    maxz = MathUtils.Clamp(edgeSize, maxz, map.Height - edgeSize - 1);

                    for (int x = minx; x <= maxx; x++)
                    {
                        for (int z = minz; z <= maxz; z++)
                        {
                            clearCells[x, z] = true;
                            map.AddBits(x, z, CellIndex.Walls, 1 << MapWallBits.IsRoomBitOffset);
                            roomIds[x, z] = i + 1;
                        }
                    }
                }

                List<ConnectedPairData> newPaths = _lineGenService.ConnectPoints(connectPoints, rand, 0.7f);

                foreach (ConnectedPairData cpd in newPaths)
                {
                    List<PointXZ> newLine = _lineGenService.GridConnect((int)cpd.Point1.X, (int)cpd.Point1.Z,
                        (int)cpd.Point2.X, (int)cpd.Point2.Z, rand.NextDouble() < 0.5f);


                    foreach (PointXZ p in newLine)
                    {
                        if (p.X > edgeSize && p.Z > edgeSize && p.X < map.Width - edgeSize && p.Z < map.Height - edgeSize)
                        {
                            clearCells[p.X, p.Z] = true;
                        }
                    }
                }

                for (int x = 0; x < map.Width; x++)
                {
                    for (int z = 0; z < map.Height; z++)
                    {

                        if (clearCells[x, z])
                        {
                            map.Set(x, z, CellIndex.Terrain, genData.ZoneType.IdKey);
                        }

                        int wallValue = 0;
                        int leftx = (x + map.Width - 1) % map.Width;
                        int rightx = (x + 1) % map.Width;
                        int upz = (z + 1) % map.Height;
                        int downz = (z + map.Height - 1) % map.Height;

                        if (clearCells[x, z])
                        {
                            wallValue = map.Get(x, z, CellIndex.Walls);
                            if (!clearCells[rightx, z])
                            {
                                wallValue |= (WallTypes.Wall << MapWallBits.EWallStart);
                            }
                            if (!clearCells[x, upz])
                            {
                                wallValue |= (WallTypes.Wall << MapWallBits.NWallStart);
                            }
                            map.AddBits(x, z, CellIndex.Walls, wallValue);

                            if (!clearCells[leftx, z])
                            {
                                byte currWallValue = map.Get(leftx, z, CellIndex.Walls);
                                currWallValue |= (WallTypes.Wall << MapWallBits.EWallStart);
                                map.AddBits(leftx, z, CellIndex.Walls, currWallValue);
                            }
                            if (!clearCells[x, downz])
                            {
                                byte currWallValue = map.Get(x, downz, CellIndex.Walls);
                                currWallValue |= (WallTypes.Wall << MapWallBits.NWallStart);
                                map.AddBits(x, downz, CellIndex.Walls, currWallValue);
                            }
                        }
                    }
                }
                PointXZ entrancePoint = roomCenters[rand.Next() % roomCenters.Count];
                enterX = entrancePoint.X;
                enterZ = entrancePoint.Z;
                roomCenters.Remove(entrancePoint);
                PointXZ exitPoint = roomCenters[rand.Next() % roomCenters.Count];
                exitX = exitPoint.X;
                exitZ = exitPoint.Z;

            }

            List<PointXZ> entranceExitPoints = new List<PointXZ>();
            entranceExitPoints.Add(new PointXZ(enterX, enterZ));
            entranceExitPoints.Add(new PointXZ(exitX, exitZ));

            MarkTilesNearEntrances(genData, map, entranceExitPoints);

            if (genData.CurrFloor < genData.MaxFloor)
            {
                long currMapId = genData.FromMapId;
                int currFromX = genData.FromMapX;
                int currFromZ = genData.FromMapZ;

                genData.FromMapId = map.IdKey;
                genData.FromMapX = exitX;
                genData.FromMapZ = exitZ;

                await _mapGenService.Generate(party, world, genData, token);

                genData.FromMapId = currMapId;
                genData.FromMapX = currFromX;
                genData.FromMapZ = currFromZ;
            }


            List<PointXZ> validEmptyCells = new List<PointXZ>();

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    if (map.IsValidEmptyCell(x, z))
                    {
                        validEmptyCells.Add(new PointXZ(x, z));
                    }
                }
            }

            AddEncounters(genData, map, validEmptyCells, rand);

            AddQuestItemLocations(genData, map, validEmptyCells, rand);

            AddMagicLocations(genData, map, validEmptyCells, rand);

            AddTeleportSquares(genData, map, validEmptyCells, rand);

            ModifyZoneTypes(genData, map, roomIds, rand);

            AddRoomDoors(genData, map, roomIds, rand);

            return new NewCrawlerMap() { Map = map, EnterX = enterX, EnterZ = enterZ };
        }

        protected void AddMagicLocations(CrawlerMapGenData genData, CrawlerMap map, List<PointXZ> validEmptyCells, IRandom rand)
        {
            List<PointXZ> removeList = new List<PointXZ>();
            IReadOnlyList<MapMagicType> mapMagics = _gameData.Get<MapMagicSettings>(_gs.ch).GetData();
            double specialTilechance = genData.GenType.SpecialTileChance;
            foreach (PointXZ pt in validEmptyCells)
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
                            if (FlagUtils.IsSet(bits, (1 << MapMagics.Peaceful)) &&
                                FlagUtils.IsSet(bits, (1 << MapMagics.NoMagic)))
                            {
                                continue;
                            }
                            bits |= (1 << (int)(mtype.IdKey - 1));

                            map.SetEntity(xx, zz, EntityTypes.MapMagic, bits);
                            PointXZ magicPt = validEmptyCells.FirstOrDefault(p => p.X == xx && p.Z == zz);
                            if (!removeList.Contains(magicPt))
                            {
                                removeList.Add(magicPt);
                            }
                        }
                    }
                }
            }
            foreach (PointXZ removePt in removeList)
            {
                validEmptyCells.Remove(removePt);
            }


        }

        protected void AddQuestItemLocations(CrawlerMapGenData genData, CrawlerMap map, List<PointXZ> validEmptyCells, IRandom rand)
        {
            for (int i = 0; i < 3; i++)
            {
                if (validEmptyCells.Count < 1)
                {
                    break;
                }
                PointXZ pt = validEmptyCells[rand.Next(validEmptyCells.Count)];
                validEmptyCells.Remove(pt);
                map.SetEntity(pt.X, pt.Z, EntityTypes.QuestItem, byte.MaxValue);
            }
        }

        protected void MarkTilesNearEntrances(CrawlerMapGenData genData, CrawlerMap map, List<PointXZ> entranceExitPoints)
        {

            foreach (PointXZ point in entranceExitPoints)
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

        protected void AddEncounters(CrawlerMapGenData genData, CrawlerMap map, List<PointXZ> validEmptyCells, IRandom rand)
        {
            MapEncounterSettings encounterSettings = _gameData.Get<MapEncounterSettings>(_gs.ch);

            int encountersToPlace = (int)(validEmptyCells.Count * encounterSettings.EncounterChance);

            int encounterTries = encountersToPlace * 20;

            for (int i = 0; i < encounterTries && encountersToPlace > 0; i++)
            {
                if (validEmptyCells.Count < 1)
                {
                    continue;
                }

                PointXZ pt = validEmptyCells[rand.Next() % validEmptyCells.Count];
                validEmptyCells.Remove(pt);

                map.SetEntity(pt.X, pt.Z, EntityTypes.MapEncounter, GetRandomEncounter(rand));
                encountersToPlace--;
            }
        }

        protected long GetRandomEncounter(IRandom rand)
        {

            MapEncounterType encounter = RandomUtils.GetRandomElement(_gameData.Get<MapEncounterSettings>(_gs.ch).GetData(), rand);

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

        protected void ConnectOpenCells(CrawlerMap map, CrawlerMapGenData genData, IRandom rand)
        {

            bool[,] openCell = new bool[map.Width, map.Height];

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    openCell[x, z] = map.Get(x, z, CellIndex.Terrain) >= 0;
                }
            }

            while (true)
            {
                bool hadDisconnectedCell = false;

                bool[,] connectedCells = new bool[map.Width, map.Height];

                Queue<PointXZ> cellsToCheck = new Queue<PointXZ>();

                cellsToCheck.Enqueue(new PointXZ(map.Width / 2, map.Height / 2));

                while (cellsToCheck.Count > 0)
                {
                    PointXZ currentCell = cellsToCheck.Dequeue();

                    int x = currentCell.X;
                    int z = currentCell.Z;

                    connectedCells[x, z] = true;

                    // If x on right or map loops, see if there's a disconnected cell to east.
                    if (x < map.Width - 1 || map.HasFlag(CrawlerMapFlags.IsLooping))
                    {
                        int nx = (x + 1) % map.Width;
                        if (!connectedCells[nx, z] && !WallTypes.IsBlockingType(map.EastWall(x, z)))
                        {
                            connectedCells[nx, z] = true;
                            cellsToCheck.Enqueue(new PointXZ(nx, z));
                        }
                    }
                    if (x > 0 || map.HasFlag(CrawlerMapFlags.IsLooping))
                    {
                        int nx = (x - 1 + map.Width) % map.Width;
                        if (!connectedCells[nx, z] && !WallTypes.IsBlockingType(map.EastWall(nx, z)))
                        {
                            connectedCells[nx, z] = true;
                            cellsToCheck.Enqueue(new PointXZ(nx, z));
                        }
                    }

                    if (z < map.Height - 1 || map.HasFlag(CrawlerMapFlags.IsLooping))
                    {
                        int nz = (z + 1) % map.Height;
                        if (!connectedCells[x, nz] && !WallTypes.IsBlockingType(map.NorthWall(x, z)))
                        {
                            connectedCells[x, nz] = true;
                            cellsToCheck.Enqueue(new PointXZ(x, nz));
                        }
                    }
                    if (z > 0 || map.HasFlag(CrawlerMapFlags.IsLooping))
                    {
                        int nz = (z - 1 + map.Height) % map.Height;
                        if (!connectedCells[x, nz] && !WallTypes.IsBlockingType(map.NorthWall(x, nz)))
                        {
                            connectedCells[x, nz] = true;
                            cellsToCheck.Enqueue(new PointXZ(x, nz));
                        }
                    }
                }

                for (int x = 0; x < map.Width; x++)
                {
                    for (int z = 0; z < map.Height; z++)
                    {
                        if (openCell[x, z] && !connectedCells[x, z])
                        {
                            hadDisconnectedCell = true;

                            if (rand.NextDouble() > 0.1f)
                            {
                                continue;
                            }

                            long bits = map.Get(x, z, CellIndex.Walls);

                            if (x < map.Width - 1 || genData.Looping)
                            {
                                bits &= ~(WallTypes.Wall << MapWallBits.EWallStart);
                            }
                            if (z < map.Height - 1 || genData.Looping)
                            {
                                bits &= ~(WallTypes.Wall << MapWallBits.NWallStart);
                            }

                            map.Set(x, z, CellIndex.Walls, (byte)bits);

                            if (x > 0 || genData.Looping)
                            {
                                int nx = (x + map.Width - 1) % map.Width;

                                long ebits = map.Get(nx, z, CellIndex.Walls);

                                ebits &= ~(WallTypes.Wall << MapWallBits.EWallStart);
                                map.Set(nx, z, CellIndex.Walls, (byte)ebits);
                            }
                            if (z > 0 || genData.Looping)
                            {
                                int nz = (z + map.Height - 1) % map.Height;
                                long nbits = map.Get(x, nz, CellIndex.Walls);
                                nbits &= ~(WallTypes.Wall << MapWallBits.NWallStart);
                                map.Set(x, nz, CellIndex.Walls, (byte)nbits);
                            }

                        }
                    }
                }

                if (!hadDisconnectedCell)
                {
                    break;
                }
            }
        }


        private bool[] GetBlockedDirs(CrawlerMap map, MapDir[] mapDirs, int x, int z)
        {
            bool[] isBlocked = new bool[mapDirs.Length];
            for (int d = 0; d < mapDirs.Length; d++)
            {
                MapDir dir = mapDirs[d];

                int blockingBits = _crawlerMapService.GetBlockingBits(map, x, z, x + dir.DX, z + dir.DZ, false);

                isBlocked[d] = WallTypes.IsBlockingType(blockingBits);
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

        private void AddTeleportSquares(CrawlerMapGenData genData, CrawlerMap map, List<PointXZ> validEmptyCells, IRandom rand)
        {

            List<PointXZ> teleportEntryPoints = new List<PointXZ>();
            MapDir[] mapDirs = MapDirs.GetDirs();

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
                            MapDir currDir = mapDirs[d];
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
                                        MapDir prevDir = mapDirs[otherDirIndex];
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

                    if (canBeTeleport && !map.Details.Any(d => d.X == x && d.Z == z))
                    {
                        teleportEntryPoints.Add(new PointXZ(x, z));
                    }
                }
            }

            int entryPointCount = teleportEntryPoints.Count;
            List<PointXZ> teleportExitPoints = new List<PointXZ>();

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    if (map.IsValidEmptyCell(x, z) &&
                        !teleportEntryPoints.Any(t => t.X == x && t.Z == z)
                        )
                    {
                        teleportExitPoints.Add(new PointXZ((int)x, (int)z));
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
            teleportEntryPoints = teleportEntryPoints.OrderBy(x => HashUtils.NewUUId()).ToList();

            List<List<PointXZ>> allCheckLists = new List<List<PointXZ>>() { teleportEntryPoints, teleportExitPoints };

            while (teleportQuantityLeft > 0 && teleportEntryPoints.Count > 0 && teleportExitPoints.Count > 0)
            {
                PointXZ enterPoint = teleportEntryPoints[rand.Next() % teleportEntryPoints.Count];

                teleportEntryPoints.Remove(enterPoint);

                PointXZ currPoint = validEmptyCells.FirstOrDefault(p => p.X == enterPoint.X && p.Z == enterPoint.Z);
                if (currPoint != null)
                {
                    validEmptyCells.Remove(currPoint);
                }


                List<PointXZ> okPoints = new List<PointXZ>();

                foreach (PointXZ teleExitPoint in teleportExitPoints)
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

                PointXZ exitPt = okPoints[rand.Next() % okPoints.Count];

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
                List<PointXZ> badPoints = new List<PointXZ>() { enterPoint, exitPt };

                foreach (PointXZ bp in badPoints)
                {
                    foreach (List<PointXZ> checkList in allCheckLists)
                    {
                        List<PointXZ> removeList = new List<PointXZ>();
                        foreach (PointXZ op in checkList)
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

                        foreach (PointXZ removeMe in removeList)
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

        private void AddRandomRooms(PartyData party, CrawlerMapGenData genData, CrawlerMap map, int[,] roomIds, IRandom rand)
        {
            if (!genData.RandomWallsDungeon)
            {
                return;
            }

            int size = map.Width * map.Height;

            double roomCountFloat = size / 40.0;

            int roomCount = (int)roomCountFloat;
            if (rand.NextDouble() < (roomCountFloat - roomCount))
            {
                roomCount++;
            }

            float minDiv = 8.0f;
            float maxDiv = 4.0f;

            float doorChance = 0.2f;

            double minWidth = map.Width / minDiv;
            double maxWidth = map.Width / maxDiv;

            double minHeight = map.Height / minDiv; ;
            double maxHeight = map.Height / maxDiv;

            for (int r = 0; r < roomCount; r++)
            {
                double widthFloat = Math.Min(MathUtils.FloatRange(minWidth, maxWidth, rand), MathUtils.FloatRange(minWidth, maxWidth, rand));
                double heightFloat = Math.Min(MathUtils.FloatRange(minHeight, maxHeight, rand), MathUtils.FloatRange(minHeight, maxHeight, rand));

                int width = (int)widthFloat;
                int height = (int)heightFloat;

                if (rand.NextDouble() < (widthFloat - width))
                {
                    width++;
                }

                if (rand.NextDouble() < (heightFloat - height))
                {
                    height++;
                }

                int xstart = MathUtils.IntRange(1, map.Width - width - 2, rand);
                int zstart = MathUtils.IntRange(1, map.Height - height - 2, rand);

                int xend = xstart + width;
                int zend = zstart + height;

                for (int x = xstart - 1; x <= xend; x++)
                {
                    for (int z = zstart - 1; z <= zend; z++)
                    {

                        if (roomIds[x, z] == 0)
                        {
                            roomIds[x, z] = (r + 1);
                        }
                        int northBits = map.NorthWall(x, z);
                        int eastBits = map.EastWall(x, z);

                        int walls = map.Get(x, z, CellIndex.Walls);

                        if (x == xstart - 1 || x == xend)
                        {
                            if (rand.NextDouble() < doorChance)
                            {
                                eastBits = WallTypes.Door;
                            }
                            else
                            {
                                eastBits = WallTypes.Wall;
                            }
                        }
                        else
                        {
                            eastBits = 0;
                        }
                        if (z == zstart - 1 || z == zend)
                        {
                            if (rand.NextDouble() < doorChance)
                            {
                                northBits = WallTypes.Door;
                            }
                            else
                            {
                                northBits = WallTypes.Wall;
                            }
                        }
                        else
                        {
                            northBits = 0;
                        }

                        int finalBits = northBits << MapWallBits.NWallStart |
                            eastBits << MapWallBits.EWallStart;

                        map.Set(x, z, CellIndex.Walls, finalBits);
                    }
                }
            }
        }

        private void ModifyZoneTypes(CrawlerMapGenData genData, CrawlerMap map, int[,] roomIds, IRandom rand)
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

                long replaceZoneTypeId = zoneTypes[rand.Next() % zoneTypes.Count];
                zoneTypes.Remove(replaceZoneTypeId);

                for (int x = 0; x < map.Width; x++)
                {
                    for (int z = 0; z < map.Height; z++)
                    {
                        if (roomIds[x, z] == roomId)
                        {
                            if (map.Get(x, z, CellIndex.Terrain) > 0)
                            {
                                map.Set(x, z, CellIndex.Terrain, replaceZoneTypeId);
                            }
                        }
                    }
                }
            }
        }

        // Try to add doors between rooms and non-rooms.
        private void AddRoomDoors(CrawlerMapGenData genData, CrawlerMap map, int[,] roomIds, IRandom rand)
        {
            if (genData.RandomWallsDungeon)
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

        private void ClearNEEntrances(int x, int z, bool[,] entrances)
        {
            entrances[x, z] = false;
            entrances[x + 1, z] = false;
            entrances[x, z + 1] = false;
        }
    }
}

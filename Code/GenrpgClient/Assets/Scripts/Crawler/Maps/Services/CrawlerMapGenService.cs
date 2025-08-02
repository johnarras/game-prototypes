using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.MapGen.Entities;
using Genrpg.Shared.Crawler.MapGen.Helpers;
using Genrpg.Shared.Crawler.MapGen.Services;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
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

namespace Assets.Scripts.Crawler.Maps
{


    public class CrawlerMapGenService : ICrawlerMapGenService
    {
        private ILogService _logService = null;
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IClientRandom _rand = null;
        private ICrawlerMapService _mapService = null;

        private CancellationToken _token;

        private PartyData _party;
        private CrawlerWorld _world;

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

            IClientRandom rand = new ClientRandom(world.MaxMapId + 3 + world.Seed / 3);

            genData.MapType = mtype;
            if (genData.GenType == null)
            {
                genData.GenType = RandomUtils.GetRandomElement(mtype.GenTypes, rand);
            }

            if (genData.ZoneType == null)
            {
                if (genData.GenType != null && genData.GenType.WeightedZones.Count > 0)
                {
                    long zoneTypeId = RandomUtils.GetRandomElement(genData.GenType.WeightedZones, rand).ZoneTypeId;

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
                            ZoneUnitKeyword zk = RandomUtils.GetRandomElement(zoneKeywords, rand);
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
                    _logService.Info("No ZoneType");
                    return null;
                }
            }

            if (genData.BuildingArtId == 0)
            {
                genData.BuildingArtId = RandomUtils.GetRandomElement(_gameData.Get<BuildingArtSettings>(_gs.ch).GetData(), rand).IdKey;
            }

            if (genData.ArtSeed == 0)
            {
                genData.ArtSeed = _rand.Next(1000000000); // Use global rand here to make it random each time we generate
            }

            ICrawlerMapGenHelper helper = GetGenHelper(genData.MapTypeId);
            NewCrawlerMap newMap = await helper.Generate(party, world, genData, token);

            SetObjectDirections(newMap.Map, rand);

            if (genData.FromMapId > 0 && newMap.EnterX >= 0 && newMap.EnterZ >= 0)
            {
                LinkTwoMaps(world, genData.FromMapId, genData.FromMapX, genData.FromMapZ, newMap.Map.IdKey, newMap.EnterX, newMap.EnterZ);
            }

            return newMap.Map;
        }

        private void LinkTwoMaps(CrawlerWorld world, long fromMapId, int fromMapX, int fromMapZ, long toMapId, int toMapX, int toMapZ)
        {
            OneWayLink(world, fromMapId, fromMapX, fromMapZ, toMapId, toMapX, toMapZ);
            OneWayLink(world, toMapId, toMapX, toMapZ, fromMapId, fromMapX, fromMapZ);
        }

        private void OneWayLink(CrawlerWorld world, long fromMapId, int fromX, int fromZ, long toMapId, int toX, int toZ)
        {
            CrawlerMap fromMap = world.GetMap(fromMapId);
            CrawlerMap toMap = world.GetMap(toMapId);

            if (fromMap == null || toMap == null)
            {
                return;
            }

            List<MyPoint2> nearbyRoads = new List<MyPoint2>();

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
                    nearbyRoads.Add(new MyPoint2(xx, zz));
                }
            }

            if (nearbyRoads.Count > 0)
            {

                long index = fromMapId + toMapId + fromX + fromZ + toX + toZ;
                MyPoint2 chosenRoad = nearbyRoads[(int)(index % nearbyRoads.Count)];

                toX = (int)chosenRoad.X;
                toZ = (int)chosenRoad.Y;
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
            MapDir[] dirs = MapDirs.GetDirs();

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    if (map.GetEntityId(x, z, EntityTypes.Building) > 0)
                    {
                        continue;
                    }

                    List<int> allBlockingBits = new List<int>();

                    foreach (MapDir dir in dirs)
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
                        map.Set(x, z, CellIndex.Dir, openDirs[_rand.Next(openDirs.Count)]);
                    }
                    else if (doorDirs.Count > 0)
                    {
                        map.Set(x, z, CellIndex.Dir, doorDirs[_rand.Next(doorDirs.Count)]);
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

    }
}

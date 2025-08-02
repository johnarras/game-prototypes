using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.MapGen.Services;
using Genrpg.Shared.Crawler.MapGen.Helpers;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.LoadSave.Constants;
using Genrpg.Shared.LoadSave.Services;
using Assets.Scripts.Repository;
using Assets.Scripts.Repository.Constants;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Zones.Constants;
using Genrpg.Shared.Units.Settings;
using System.Threading;
using Genrpg.Shared.Riddles.EntranceRiddleHelpers;

namespace Assets.Scripts.Crawler.Maps.Services
{
    public class CrawlerWorldService : ICrawlerWorldService
    {
        private IRepositoryService _repoService;
        private IGameData _gameData;
        private ICrawlerMapService _mapService;
        private ICrawlerMapGenService _mapGenService;
        private ICrawlerService _crawlerService;
        private ILogService _logService;
        private ILootGenService _lootGenService;
        private IClientRandom _rand;
        private IDispatcher _dispatcher;
        private IClientAppService _clientAppService;
        private IClientGameState _gs;
        private IPartyService _partyService;
        private ILoadSaveService _loadSaveService;
        private ITextSerializer _serializer;

        private CrawlerWorld _world = null;

        private CancellationTokenSource _source = null;
        public void SetGameToken(CancellationToken token)
        {
            _source = CancellationTokenSource.CreateLinkedTokenSource(token);
        }

        public async Task<CrawlerWorld> GenerateWorld(PartyData party)
        {

            long oldWorldId = party.WorldId;
            _partyService.ResetMaps(party);

            CrawlerWorld world = await GenerateInternal(party.WorldId, _source.Token);

            CrawlerMap firstCityMap = world.Maps.FirstOrDefault(x => x.CrawlerMapTypeId == CrawlerMapTypes.City);


            for (int i = LoadSaveConstants.MinSlot; i <= LoadSaveConstants.MaxSlot; i++)
            {
                PartyData slotData = _crawlerService.LoadParty(i);

                if (slotData != null && slotData.WorldId == oldWorldId)
                {
                    _partyService.ResetMaps(slotData);
                    slotData.CurrPos.MapId = 0;
                    await _crawlerService.SaveGame();
                }
            }

            await _crawlerService.SaveGame();

            _dispatcher.Dispatch(new UpdateCrawlerUI());
            return world;
        }

        public CrawlerMap CreateMap(CrawlerMapGenData genData, int width, int height)
        {
            long mapId = ++genData.World.MaxMapId;
            CrawlerMap map = new CrawlerMap()
            {
                Id = "Map" + mapId,
                CrawlerMapTypeId = genData.MapTypeId,
                Width = width,
                Height = height,
                Level = genData.Level,
                IdKey = mapId,
                MapFloor = genData.CurrFloor,
                ArtSeed = genData.ArtSeed,
                ZoneTypeId = genData.ZoneType.IdKey,
                BuildingTypeId = genData.ZoneType.BuildingTypeId,
                WeatherTypeId = genData.ZoneType.WeatherTypeId,
                BuildingArtId = genData.BuildingArtId,
            };

            if (genData.GenType.IsIndoors)
            {
                map.AddFlags(CrawlerMapFlags.IsIndoors);
            }
            if (genData.Looping)
            {
                map.AddFlags(CrawlerMapFlags.IsLooping);
            }


            if (genData.BaseCrawlerMapId == 0)
            {
                genData.BaseCrawlerMapId = map.IdKey;
            }

            map.BaseCrawlerMapId = genData.BaseCrawlerMapId;

            map.SetupDataBlocks();
            genData.World.AddMap(map);
            if (genData.ZoneType.ZoneUnitSpawns.Count > 0)
            {
                List<ZoneUnitSpawn> spawns = genData.ZoneType.ZoneUnitSpawns.Where(x => x.Weight > 0).OrderBy(x => x.Weight).ToList();

                if (spawns.Count > 0)
                {
                    CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

                    int spawnCount = MathUtils.IntRange(mapSettings.MinZoneUnitSpawns, mapSettings.MaxZoneUnitSpawns, _rand);

                    int sharedZoneSpawnCount = mapSettings.SharedZoneUnitCount;

                    for (int i = 0; i < sharedZoneSpawnCount && i < genData.SharedUnits.Count; i++)
                    {
                        map.ZoneUnits.Add(genData.SharedUnits[i]);
                        spawnCount--;
                    }

                    List<long> currentUnits = map.ZoneUnits.Select(x=>x.UnitTypeId).ToList();

                    spawns = spawns.Where(x=>!currentUnits.Contains(x.UnitTypeId)).ToList();    

                    double minWeight = spawns.Min(x => x.Weight);

                    List<ZoneUnitSpawn> rareSpawns = spawns.Where(x => x.Weight <= minWeight * 2).ToList();

                    for (int i = 0; i < mapSettings.RareSpawnCount; i++)
                    {
                        if (rareSpawns.Count > 0)
                        {
                            ZoneUnitSpawn rare = rareSpawns[_rand.Next() % rareSpawns.Count];
                            rareSpawns.Remove(rare);
                            spawns.Remove(rare);
                            map.ZoneUnits.Add(rare);
                        }
                    }

                    while (map.ZoneUnits.Count < spawnCount && spawns.Count > 0)
                    {
                        ZoneUnitSpawn spawn = RandomUtils.GetRandomElement(spawns, _rand);

                        spawns.Remove(spawn);
                        map.ZoneUnits.Add(spawn);
                    }

                    map.ZoneUnits = map.ZoneUnits.OrderBy(x => HashUtils.NewUUId()).ToList();

                    if (map.ZoneUnits.Count > 0 && genData.SharedUnits.Count < 1)
                    {
                        for (int i = 0; i < map.ZoneUnits.Count && i < sharedZoneSpawnCount; i++)
                        {
                            genData.SharedUnits.Add(map.ZoneUnits[i]);
                        }
                    }
                }
            }

            map.UnitKeywords = genData.UnitKeywords.ToList();

            return map;
        }

        public CrawlerMap GetMap(long mapId)
        {
            return _world?.GetMap(mapId) ?? null;
        }

        public async Task<CrawlerWorld> GetWorld(long worldId)
        {

            if (_world != null && _world.IdKey == worldId)
            {
                return _world;
            }

            CrawlerWorld world = null;

            try
            {
                world = await LoadWorld(worldId);
            }
            catch (Exception ex)
            {
                _logService.Warning("Bad map load: " + ex.Message);
            }

            if (world == null || world.IdKey != worldId)
            {
                world = await GenerateInternal(worldId, _source.Token);
            }

            _world = world;
            return world;
        }

        public async Task SaveWorld(CrawlerWorld world)
        {
            ClientRepositoryService clientRepoService = _repoService as ClientRepositoryService;

            await clientRepoService.StringSave<CrawlerWorld>(world.Id, _serializer.SerializeToString(world));
        }

        private async Task<CrawlerWorld> LoadWorld(long worldId)
        {
            ClientRepositoryService clientRepoService = _repoService as ClientRepositoryService;

            return await clientRepoService.LoadObjectFromString<CrawlerWorld>("World" + worldId);
        }

        private async Task<CrawlerWorld> GenerateInternal(long worldId, CancellationToken token)
        {

            PartyData party = _crawlerService.GetParty();
            try
            {
                CrawlerWorld world = new CrawlerWorld() { Id = "World" + worldId, Name = "World" + worldId, IdKey = worldId, Seed = _rand.Next() };
                _world = world;
                ICrawlerMapGenHelper helper = _mapGenService.GetGenHelper(CrawlerMapTypes.Outdoors);

                MyRandom rand = new MyRandom(worldId + 1);

                CrawlerMapGenData genData = new CrawlerMapGenData()
                {
                    MapTypeId = CrawlerMapTypes.Outdoors,
                    World = world,
                    Level = 1,
                    Looping = false,
                };

                CrawlerMap outdoorMap = await _mapGenService.Generate(_crawlerService.GetParty(), world, genData, token);

                string path = _clientAppService.PersistentDataPath +
                    ClientRepositoryConstants.GetDataPathPrefix() + ClientRepositoryConstants.WorldPathPrefix + worldId;
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                await SaveWorld(world);

                await _crawlerService.SaveGame();
                _crawlerService.ClearAllStates();
                _mapService.CleanMap();

                _dispatcher.Dispatch(new ClearCrawlerTilemaps());

                return world;
            }
            catch (Exception e)
            {
                _logService.Exception(e, "CrawlerWorldGen");
            }
            return null;
        }

        public async Task<ZoneType> GetCurrentZone(PartyData party, long mapId = 0, int x = -1, int z = -1)
        {

            CrawlerWorld world = await GetWorld(party.WorldId);

            if (mapId < 1)
            {
                mapId = party.CurrPos.MapId;
            }
            if (x < 0)
            {
                x = party.CurrPos.X;
            }
            if (z < 0)
            {
                z = party.CurrPos.Z;
            }

            CrawlerMap map = world.GetMap(mapId);

            if (map == null)
            {
                return null;
            }

            IReadOnlyList<ZoneType> allZoneTypes = _gameData.Get<ZoneTypeSettings>(_gs.ch).GetData();

            if (map.ZoneTypeId > 0)
            {
                return allZoneTypes.FirstOrDefault(zt => zt.IdKey == map.ZoneTypeId);
            }

            int index = map.GetIndex(x,z);

            if (map.CrawlerMapTypeId != CrawlerMapTypes.Outdoors)
            {
                return allZoneTypes.FirstOrDefault(zt => zt.IdKey > 0);
            }

            ZoneType ztype = allZoneTypes.FirstOrDefault(zt=>zt.IdKey == map.Get(x,z, CellIndex.Terrain));

            if (ztype == null || ztype.ZoneUnitSpawns.Count < 1)
            {
                return allZoneTypes.FirstOrDefault(zt => zt.IdKey > 0);
            }

            return ztype;

        }

        public async Task<int> GetMapLevelAtParty(PartyData party)
        {
            return await GetMapLevelAtPoint(await GetWorld(party.WorldId), party.CurrPos.MapId, party.CurrPos.X, party.CurrPos.Z);
        }

        public async Task<int> GetMapLevelAtPoint(CrawlerWorld world, long mapId, int x, int z)
        {
            CrawlerMap map = world.GetMap(mapId);
            
            if (map == null)
            {
                return 1;
            }

            if (map.CrawlerMapTypeId != CrawlerMapTypes.Outdoors)
            {
                return map.Level;
            }

            List<MapCellDetail> startDetails = map.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

            List<MapCellDetail> finalDetails = new List<MapCellDetail>();

            foreach (MapCellDetail detail in startDetails)
            {
                CrawlerMap cityMap = world.GetMap(detail.EntityId);

                if (cityMap != null && cityMap.CrawlerMapTypeId == CrawlerMapTypes.City)
                {
                    finalDetails.Add(detail);
                }
            }
            
            Dictionary<MapCellDetail, long> cityDistances = new Dictionary<MapCellDetail, long>();

            double px = x;
            double pz = z;

            foreach (MapCellDetail detail in finalDetails)
            {
                double distance = Math.Sqrt((px - detail.X) * (px - detail.X) + (pz - detail.Z) * (pz - detail.Z));

                cityDistances[detail] = (long)distance;  
            }

            List<long> orderedDistances = cityDistances.Values.OrderBy(x => x).ToList();

            MapCellDetail firstDetail = null;
            long firstDist = 0;
            int firstLevel = 0;
            MapCellDetail secondDetail = null;
            long secondDist = 0;
            int secondLevel = 0;

            firstDist = orderedDistances[0];
            secondDist = orderedDistances[1];

            foreach (MapCellDetail detail in cityDistances.Keys)
            {
                if (cityDistances[detail] == firstDist)
                {
                    firstDetail = detail; 
                    CrawlerMap cityMap = world.GetMap(detail.EntityId);
                    firstLevel = cityMap.Level;
                }
                if (cityDistances[detail] == secondDist)
                {
                    secondDetail = detail;
                    CrawlerMap cityMap = world.GetMap(detail.EntityId);
                    secondLevel = cityMap.Level;
                }
            }

            if (firstDist <= 0 && secondDist <= 0)
            {
                return firstLevel;
            }

            double distanceSum = firstDist + secondDist;

            double firstDistPct = firstDist / distanceSum;
            double secondDistPct = secondDist / distanceSum;

            await Task.CompletedTask;
            return (int)(firstDistPct * firstLevel + secondDistPct * secondLevel);
        }

        public async Task<List<ZoneUnitSpawn>> GetSpawnsAtPoint(PartyData party, long mapId, int x, int z)
        {
            CrawlerMap map = GetMap(mapId);

            if(map != null && map.ZoneUnits.Count > 0)
            {
                return map.ZoneUnits.ToList();
            }

            ZoneType zoneType = await GetCurrentZone(party, mapId, x, z);

            if (zoneType != null && zoneType.ZoneUnitSpawns.Count > 0)
            {
                return zoneType.ZoneUnitSpawns.ToList();
            }

            IReadOnlyList<UnitType> allUnits = _gameData.Get<UnitTypeSettings>(_gs.ch).GetData();

            List<ZoneUnitSpawn> spawns = new List<ZoneUnitSpawn>();

            foreach (UnitType utype in allUnits)
            {
                spawns.Add(new ZoneUnitSpawn() { UnitTypeId = utype.IdKey, Weight = 1 });
            }
            return spawns;
        }
    }
}

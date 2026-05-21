using OxDb.ServerCore.DataStores.Services;
using OxDb.ServerGame.Maps;
using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedGame.MapObjects.MapObjectAddons.Entities;
using OxDb.SharedGame.Spawns.WorldData;
using OxDb.SharedGame.Units.Entities;

namespace OxDb.ServerGame.MapSpawns
{

    public interface IMapSpawnDataService : IInitializable
    {
        Task SaveMapSpawnData(IFullRepositoryService repoService, MapSpawnData data, string mapId, int mapVersion);
        Task<MapSpawnData> LoadMapSpawnData(IFullRepositoryService repoService, string mapId, int mapVersion);
    }

    public class MapSpawnDataService : IMapSpawnDataService
    {
        private IFullRepositoryService _repoService = null;
        private IMapDataService _mapDataService = null;
        private ITextSerializer _serializer = null;
        public async Task Initialize(CancellationToken token)
        {
            CreateIndexData data = new CreateIndexData(typeof(UnitStatus));
            data.Configs.Add(new IndexConfig() { MemberName = nameof(UnitStatus.MapId) });
            List<Task> tasks = new List<Task>();
            tasks.Add(_repoService.CreateIndexes(data));
            data = new CreateIndexData(typeof(MapSpawn));
            data.Configs.Add(new IndexConfig() { MemberName = nameof(MapSpawn.MapId) });
            tasks.Add(_repoService.CreateIndexes(data));
            await Task.WhenAll(tasks);
        }
        public async Task SaveMapSpawnData(IFullRepositoryService repoService, MapSpawnData data, string mapId, int mapVersion)
        {
            await repoService.DeleteAll<MapSpawn>(x => x.MapId == mapId);
            string ownerId = _mapDataService.GetMapOwnerId(mapId, mapVersion);
            foreach (MapSpawn spawn in data.Data)
            {
                spawn.Id = spawn.ObjId + "-" + ownerId;
                spawn.OwnerId = ownerId;
                spawn.MapId = mapId;
                if (spawn.Addons != null)
                {
                    spawn.AddonString = _serializer.SerializeToString(spawn.Addons);
                    spawn.Addons = null;
                }
            }

            await repoService.SaveAll(data.Data);

        }

        public async Task<MapSpawnData> LoadMapSpawnData(IFullRepositoryService repoService, string mapId, int mapVersion)
        {
            MapSpawnData spawnData = new MapSpawnData();

            string mapOwnerId = _mapDataService.GetMapOwnerId(mapId, mapVersion);

            spawnData.Data = await repoService.Search<MapSpawn>(x => x.OwnerId == mapOwnerId, 1000000);

            foreach (MapSpawn mapSpawn in spawnData.Data)
            {
                if (!string.IsNullOrEmpty(mapSpawn.AddonString))
                {
                    mapSpawn.Addons = _serializer.Deserialize<List<IMapObjectAddon>>(mapSpawn.AddonString);
                    mapSpawn.AddonString = null;
                }
            }


            return spawnData;
        }

    }
}



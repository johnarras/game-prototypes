using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.DataStores.Categories.WorldData;
using OxDb.SharedGame.Interfaces;
using OxDb.SharedGame.Spawns.Entities;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Spawns.WorldData
{



    public class MapSpawnData : BaseWorldData, IStringOwnerId
    {
        public override string Id { get; set; }
        public override string Name { get; set; }
        public string OwnerId { get; set; }
        public List<MapSpawn> Data { get; set; } = new List<MapSpawn>();
        public int MaxId { get; set; } = 12345;

        public override void Delete(IRepositoryService repoSystem) { repoSystem.Delete(this); }

        public void AddSpawn(InitSpawnData initSpawnData)
        {
            MapSpawn currSpawn = Data.FirstOrDefault(sp => sp.X == initSpawnData.SpawnX && sp.Z == initSpawnData.SpawnZ);
            if (currSpawn != null)
            {
                currSpawn.EntityTypeId = initSpawnData.EntityTypeId;
                currSpawn.EntityId = initSpawnData.EntityId;
                return;
            }

            string strId = HashUtils.GetIdFromVal(MaxId++);

            MapSpawn spawn = new MapSpawn()
            {
                ObjId = strId,
                EntityTypeId = initSpawnData.EntityTypeId,
                EntityId = initSpawnData.EntityId,
                X = initSpawnData.SpawnX,
                Z = initSpawnData.SpawnZ,
                Rot = (short)initSpawnData.Rot,
                ZoneId = initSpawnData.ZoneId,
                LocationId = initSpawnData.LocationId,
                LocationPlaceId = initSpawnData.LocationPlaceId,
                SpawnSeconds = initSpawnData.SpawnSeconds,
                OverrideZonePercent = initSpawnData.ZoneOverridePercent,
                Addons = initSpawnData.Addons,
                Name = initSpawnData.Name,
            };
            Data.Add(spawn);

        }
    }
}



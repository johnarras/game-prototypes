using OxDb.SharedCore.Website.Interfaces;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.Spawns.WorldData;

namespace OxDb.SharedGame.MapServer.WebApi.UploadMap
{
    public class UploadMapRequest : IClientUserRequest
    {
        public Map Map { get; set; }
        public MapSpawnData SpawnData { get; set; }
    }
}



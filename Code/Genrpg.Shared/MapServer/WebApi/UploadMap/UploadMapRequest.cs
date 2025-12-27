using MessagePack;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.MapServer.WebApi.UploadMap
{
    public class UploadMapRequest : IClientUserRequest
    {
        public Map Map { get; set; }
        public MapSpawnData SpawnData { get; set; }
        public string WorldDataEnv { get; set; }
    }
}



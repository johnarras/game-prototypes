using OxDb.SharedGame.MapMessages;
using OxDb.SharedGame.Spawns.Interfaces;

namespace OxDb.MapServer.Maps.Messaging
{
    public sealed class RespawnObject : BaseMapMessage
    {
        public IMapSpawn Spawn { get; set; }
    }
}


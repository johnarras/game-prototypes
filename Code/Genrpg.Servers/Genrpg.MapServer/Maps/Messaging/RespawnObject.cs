using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Spawns.Interfaces;

namespace Genrpg.MapServer.Maps.Messaging
{
    public sealed class RespawnObject : BaseMapMessage
    {
        public IMapSpawn Spawn { get; set; }
    }
}


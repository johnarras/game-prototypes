using Genrpg.Shared.Networking.Constants;

namespace Genrpg.Shared.MapServer.Entities.MapCache
{

    public class CachedMapInstance
    {
        public string InstanceId { get; set; }
        public string Host { get; set; }
        public long Port { get; set; }
        public EMapApiSerializers SerializerType { get; set; }
    }
}



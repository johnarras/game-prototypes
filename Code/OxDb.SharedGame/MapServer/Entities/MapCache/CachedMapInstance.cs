using OxDb.SharedGame.Networking.Constants;

namespace OxDb.SharedGame.MapServer.Entities.MapCache
{

    public class CachedMapInstance
    {
        public string InstanceId { get; set; }
        public string Host { get; set; }
        public long Port { get; set; }
        public EMapApiSerializers SerializerType { get; set; }
    }
}



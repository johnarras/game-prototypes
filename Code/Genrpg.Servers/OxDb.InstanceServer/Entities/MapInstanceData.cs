using OxDb.SharedGame.Networking.Constants;

namespace OxDb.InstanceServer.Entities
{
    public class MapInstanceData
    {
        public string MapId { get; set; }
        public string InstanceId { get; set; }
        public string ServerName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public int Size { get; set; }
        public EMapApiSerializers SerializerType { get; set; }
    }
}



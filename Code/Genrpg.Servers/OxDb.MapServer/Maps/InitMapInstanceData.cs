using OxDb.SharedGame.Networking.Constants;

namespace OxDb.MapServer.Maps
{
    public class InitMapInstanceData
    {
        public string MapId { get; set; }
        public int Port { get; set; }
        public EMapApiSerializers SerializerType { get; set; }
    }
}



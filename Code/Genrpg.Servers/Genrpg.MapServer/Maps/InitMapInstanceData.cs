using Genrpg.Shared.Networking.Constants;

namespace Genrpg.MapServer.Maps
{
    public class InitMapInstanceData
    {
        public string MapId { get; set; }
        public int Port { get; set; }
        public EMapApiSerializers SerializerType { get; set; }
    }
}



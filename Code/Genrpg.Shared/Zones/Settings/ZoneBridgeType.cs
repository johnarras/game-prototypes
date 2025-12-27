using MessagePack;
namespace Genrpg.Shared.Zones.Settings
{
    public class ZoneBridgeType
    {
        public long BridgeTypeId { get; set; }
        public int Chance { get; set; }
        public string Name { get; set; }
        public ZoneBridgeType()
        {
        }
    }
}



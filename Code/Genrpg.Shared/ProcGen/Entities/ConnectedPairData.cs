using MessagePack;
namespace Genrpg.Shared.ProcGen.Entities
{
    // MessagePackIgnore                        
    public class ConnectedPairData
    {
        public ConnectPointData Point1;
        public ConnectPointData Point2;
        public double Distance;
    }
}

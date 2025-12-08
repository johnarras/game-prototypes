using MessagePack;
namespace Genrpg.Shared.Trader.Caravans.Entities
{
    [MessagePackObject]
    public class CaravanPosition
    {
        [Key(0)] public bool OnRoad { get; set; }
        [Key(1)] public long CurrentRoadId { get; set; }
        [Key(2)] public long CurrentCityId { get; set; }
        [Key(3)] public long DistanceTravelled { get; set; }
        [Key(4)] public long TargetCityId { get; set; }
    }
}

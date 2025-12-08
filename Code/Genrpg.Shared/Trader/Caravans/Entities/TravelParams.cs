using MessagePack;
namespace Genrpg.Shared.Trader.Caravans.Entities
{
    [MessagePackObject]
    public class TravelParams
    {
        [Key(0)] public long Days { get; set; }
        [Key(1)] public long DicePerDay { get; set; }
        [Key(2)] public long BonusPerDay { get; set; }
        [Key(3)] public long CostPerDay { get; set; }
        [Key(4)] public long TotalCost { get; set; }
    }
}

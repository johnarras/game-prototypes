using MessagePack;
namespace Genrpg.Shared.Trader.Caravans.PlayerData
{
    [MessagePackObject]
    public class CaravanAnimal
    {
        [Key(0)] public long AnimalTypeId { get; set; }
        [Key(1)] public long MaxEnergy { get; set; }
        [Key(2)] public long Energy { get; set; }
        [Key(3)] public int Encumberance { get; set; }
    }
}

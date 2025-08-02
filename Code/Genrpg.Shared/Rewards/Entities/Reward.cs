using MessagePack;
using Genrpg.Shared.Inventory.PlayerData;

namespace Genrpg.Shared.Rewards.Entities
{

    public interface IReward
    {
        long EntityTypeId { get; set; }
        long EntityId { get; set; }
        long Quantity { get; set; }
        long QualityTypeId { get; set; }
        int Level { get; set; }
        Item ExtraData { get; set; }
    }


    [MessagePackObject]
    public class Reward : IReward
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long Quantity { get; set; }
        [Key(3)] public long QualityTypeId { get; set; }
        [Key(4)] public int Level { get; set; }
        [Key(5)] public Item ExtraData { get; set; }

    }
}

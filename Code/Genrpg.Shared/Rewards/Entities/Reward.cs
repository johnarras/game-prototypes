using CommunityToolkit.HighPerformance.Helpers;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using MessagePack;

namespace Genrpg.Shared.Rewards.Entities
{

    public interface IReward : IEffect
    {
        long UniqueId { get; set; }
        Item ExtraData { get; set; }

    }

    [MessagePackObject]
    public class Reward : IReward
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long Quantity { get; set; }
        [Key(3)] public long UniqueId { get; set; }
        [Key(4)] public Item ExtraData { get; set; }


        public Reward()
        {

        }

        public Reward(IReward other)
        {
            EntityTypeId = other.EntityTypeId;
            EntityId = other.EntityId;
            Quantity = other.Quantity;
            UniqueId = other.UniqueId;
            ExtraData = other.ExtraData;
        }

        public Reward(IEffect other)
        {

            EntityTypeId = other.EntityTypeId;
            EntityId = other.EntityId;
            Quantity = other.Quantity;
            ExtraData = null!;
        }
    }
}



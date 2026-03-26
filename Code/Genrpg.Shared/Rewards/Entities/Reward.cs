using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using MessagePack;

namespace Genrpg.Shared.Rewards.Entities
{

    public interface IReward : IEffect
    {
        Item ExtraData { get; set; }

    }

    [MessagePackObject]
    public class Reward : IReward
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long Quantity { get; set; }
        [Key(3)] public Item ExtraData { get; set; }


        public Reward()
        {

        }

        public Reward(IReward other)
        {
            EntityTypeId = other.EntityTypeId;
            EntityId = other.EntityId;
            Quantity = other.Quantity;
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



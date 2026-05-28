using MessagePack;
using OxDb.SharedCore.Effects.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OxDb.SharedCore.Rewards.Entities
{

    public interface IReward : IEffect
    {
        long UniqueId { get; set; }
        object ExtraData { get; set; }

    }

    [MessagePackObject]
    public class Reward : IReward
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long Quantity { get; set; }
        [Key(3)] public long UniqueId { get; set; }
        [Key(4)] public object ExtraData { get; set; }


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



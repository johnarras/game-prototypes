using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Effects.Entities
{
    public interface IEffect
    {
        public long EntityTypeId { get; set; }

        public long Quantity { get; set; }

        public long EntityId { get; set; }
    }

    [MessagePackObject]
    public class Effect : IEffect
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long Quantity { get; set; }
    }
}

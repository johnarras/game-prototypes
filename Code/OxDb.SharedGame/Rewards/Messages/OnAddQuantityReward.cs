using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Rewards.Messages
{
    [MessagePackObject]
    public sealed class OnAddQuantityReward : BaseMapApiMessage
    {
        [Key(0)] public string CharId { get; set; }
        [Key(1)] public long EntityTypeId { get; set; }
        [Key(2)] public long EntityId { get; set; }
        [Key(3)] public long Quantity { get; set; }
        [Key(4)] public long RewardSourceId { get; set; }
    }
}



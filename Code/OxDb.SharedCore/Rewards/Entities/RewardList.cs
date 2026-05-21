using MessagePack;
using System.Collections.Generic;

namespace OxDb.SharedCore.Rewards.Entities
{
    [MessagePackObject]
    public class RewardList
    {
        [Key(0)] public string PendingRewardListId { get; set; }
        [Key(1)] public long RewardSourceId { get; set; }
        [Key(2)] public List<Reward> Rewards { get; set; } = new List<Reward>();
        [Key(3)] public long EntityId { get; set; }
    }
}



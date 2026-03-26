using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Rewards.Entities;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Loot.Messages
{
    [MessagePackObject]
    public sealed class SendRewards : BaseMapApiMessage
    {
        [Key(0)] public bool ShowPopup { get; set; }
        [Key(1)] public List<RewardList> Rewards { get; set; }
    }
}



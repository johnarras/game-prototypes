using MessagePack;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.MapMessages;
using System.Collections.Generic;

namespace OxDb.SharedGame.Loot.Messages
{
    [MessagePackObject]
    public sealed class SendRewards : BaseMapApiMessage
    {
        [Key(0)] public bool ShowPopup { get; set; }
        [Key(1)] public List<RewardList> Rewards { get; set; }
    }
}



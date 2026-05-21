using MessagePack;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.MapMessages;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;

namespace OxDb.SharedGame.Combat.Messages
{
    [MessagePackObject]
    public sealed class Died : BaseMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }
        [Key(1)] public List<RewardList> Loot { get; set; }
        [Key(2)] public List<RewardList> SkillLoot { get; set; }
        [Key(3)] public AttackerInfo FirstAttacker { get; set; }
    }
}



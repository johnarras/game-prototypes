using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Units.Entities;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Combat.Messages
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



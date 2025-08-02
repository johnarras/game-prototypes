using MessagePack;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Spells.Settings.Skills;
using Genrpg.Shared.Spells.Settings.Spells;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class SpellHit : BaseMapMessage
    {

        [Key(0)] public long Id { get; set; }

        [IgnoreMember] public Unit OrigTarget { get; set; }

        [IgnoreMember] public Unit Target { get; set; }

        [Key(1)] public int ProcDepth { get; set; }

        [Key(2)] public bool PrimaryTarget { get; set; }

        [Key(3)] public SendSpell SendSpell { get; set; }

        [Key(4)] public long BaseQuantity { get; set; }

        [Key(5)] public float CritMult { get; set; }

        [Key(6)] public float CritChance { get; set; }

        [Key(7)] public SpellEffect Effect { get; set; }

        [Key(8)] public SkillType SkillType { get; set; }
        [Key(9)] public ElementType ElementType { get; set; }

        public SpellHit()
        {
        }
    }
}

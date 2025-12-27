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
    public sealed class SpellHit : BaseMapMessage
    {

        public long Id { get; set; }

        [IgnoreMember] public Unit OrigTarget { get; set; }

        [IgnoreMember] public Unit Target { get; set; }

        public int ProcDepth { get; set; }

        public bool PrimaryTarget { get; set; }

        public SendSpell SendSpell { get; set; }

        public long BaseQuantity { get; set; }

        public float CritMult { get; set; }

        public float CritChance { get; set; }

        public SpellEffect Effect { get; set; }

        public SkillType SkillType { get; set; }
        public ElementType ElementType { get; set; }

        public SpellHit()
        {
        }
    }
}



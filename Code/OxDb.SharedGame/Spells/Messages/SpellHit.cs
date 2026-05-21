using MessagePack;
using OxDb.SharedGame.MapMessages;
using OxDb.SharedGame.Spells.Settings.Elements;
using OxDb.SharedGame.Spells.Settings.Skills;
using OxDb.SharedGame.Spells.Settings.Spells;
using OxDb.SharedGame.Units.Entities;

namespace OxDb.SharedGame.Spells.Messages
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

        public SpellEffect SpellEffect { get; set; }

        public SkillType SkillType { get; set; }
        public ElementType ElementType { get; set; }

        public SpellHit()
        {
        }
    }
}



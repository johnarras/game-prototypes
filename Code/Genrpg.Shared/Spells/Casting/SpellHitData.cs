using MessagePack;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Spells.Messages;

namespace Genrpg.Shared.Spells.Casting
{
    // MessagePackIgnore
    public class SpellHitData
    {

        public int Id { get; set; }

        public bool UpdatedStatEffect { get; set; }

        public Unit OrigTarget { get; set; }

        public Unit Target { get; set; }

        public bool PrimaryTarget { get; set; }

        public bool IsCrit { get; set; }
        public long BaseAmount { get; set; }
        public long DefenseAmount { get; set; }
        public long MaxAmount { get; set; }
        public long FinalAmount { get; set; }
        public long Amount { get; set; }
        public long AbsorbAmount { get; set; }
        public int PowerPct { get; set; }
        public SendSpell SendSpell { get; set; }

        public SpellHitData()
        {
        }
    }
}

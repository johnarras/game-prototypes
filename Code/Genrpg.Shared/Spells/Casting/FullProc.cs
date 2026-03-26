using Genrpg.Shared.Spells.Messages;

namespace Genrpg.Shared.Spells.Casting
{
    public class FullProc
    {
        public SpellHit SpellHit { get; set; }
        public OldSpellProc Proc { get; set; }
        public CurrentProc Current { get; set; }
    }
}



using OxDb.SharedGame.Spells.Messages;

namespace OxDb.SharedGame.Spells.Casting
{
    public class FullProc
    {
        public SpellHit SpellHit { get; set; }
        public OldSpellProc Proc { get; set; }
        public CurrentProc Current { get; set; }
    }
}



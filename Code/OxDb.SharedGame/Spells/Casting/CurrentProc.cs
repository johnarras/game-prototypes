using System;

namespace OxDb.SharedGame.Spells.Casting
{
    public class CurrentProc
    {
        public long SpellTypeId { get; set; }
        public DateTime CooldownEnds { get; set; }
    }
}



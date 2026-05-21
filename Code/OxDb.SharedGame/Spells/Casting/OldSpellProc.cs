namespace OxDb.SharedGame.Spells.Casting
{
    public interface IOldSpellProc
    {

        int Chance { get; set; }
        long SpellId { get; set; }
        int Cooldown { get; set; }
        long ProcTypeId { get; set; }
        long FromElementTypeId { get; set; }
        long FromSkillTypeId { get; set; }
        int Scale { get; set; }
    }

    public class OldSpellProc : IOldSpellProc
    {
        public int Chance { get; set; }
        public long SpellId { get; set; }
        public int Cooldown { get; set; }
        public long ProcTypeId { get; set; }
        public long FromElementTypeId { get; set; }
        public long FromSkillTypeId { get; set; }
        public int Scale { get; set; }



        public static OldSpellProc CreateFrom(IOldSpellProc iproc)
        {
            return new OldSpellProc()
            {
                Chance = iproc.Chance,
                SpellId = iproc.SpellId,
                Cooldown = iproc.Cooldown,
                ProcTypeId = iproc.ProcTypeId,
                FromElementTypeId = iproc.FromElementTypeId,
                FromSkillTypeId = iproc.FromSkillTypeId,
                Scale = iproc.Scale
            };
        }
    }
}



namespace OxDb.SharedGame.Stats.Entities
{

    public interface IStatPct
    {
        long StatTypeId { get; set; }
        int Percent { get; set; }
    }

    public class StatPct : IStatPct
    {
        public long StatTypeId { get; set; }
        public int Percent { get; set; }
        public string Name { get; set; }
    }
}



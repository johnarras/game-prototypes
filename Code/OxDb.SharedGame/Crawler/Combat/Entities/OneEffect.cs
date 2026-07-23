using OxDb.SharedGame.Crawler.Combat.Constants;

namespace OxDb.SharedGame.Crawler.Combat.Entities
{
    public class OneEffect
    {
        public EHitTypes HitType { get; set; }
        public long MinQuantity { get; set; }
        public long MaxQuantity { get; set; }
        public double CritChance { get; set; }
        public double PowerPercent { get; set; }
    }
}



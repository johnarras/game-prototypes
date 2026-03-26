using Genrpg.Shared.Crawler.Combat.Constants;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    public class OneEffect
    {
        public EHitTypes HitType { get; set; }
        public long MinQuantity { get; set; }
        public long MaxQuantity { get; set; }
        public double CritChance { get; set; }
        public double PowerPercent { get; set; } = 100;
    }
}



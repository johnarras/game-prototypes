using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Spells.Settings.Elements;

namespace OxDb.SharedGame.Crawler.Combat.Entities
{
    public class FullEffect
    {
        public CrawlerSpellEffect Effect { get; set; }
        public OneEffect Hit { get; set; }
        public ElementType ElementType { get; set; }
        public double Chance { get; set; }
        public bool InitialEffect { get; set; }

    }
}



using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Spells.Settings.Elements;

namespace Genrpg.Shared.Crawler.Combat.Entities
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



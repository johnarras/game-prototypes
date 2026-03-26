using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    public class CombatResults
    {
        public CrawlerCombatState StartState { get; set; }
        public List<CombatUpdate> Updates { get; set; }
        public CrawlerCombatState EndState { get; set; }
    }
}



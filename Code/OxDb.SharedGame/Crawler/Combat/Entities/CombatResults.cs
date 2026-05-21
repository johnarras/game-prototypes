using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Combat.Entities
{
    public class CombatResults
    {
        public CrawlerCombatState StartState { get; set; }
        public List<CombatUpdate> Updates { get; set; }
        public CrawlerCombatState EndState { get; set; }
    }
}



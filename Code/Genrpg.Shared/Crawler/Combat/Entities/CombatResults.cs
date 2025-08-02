using MessagePack;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    // MessagePackIgnore
    public class CombatResults
    {
        public CrawlerCombatState StartState { get; set; }
        public List<CombatUpdate> Updates { get; set; }
        public CrawlerCombatState EndState { get; set; }
    }
}

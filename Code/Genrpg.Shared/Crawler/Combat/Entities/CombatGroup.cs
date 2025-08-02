using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    // MessagePackIgnore
    public class CombatGroup
    {
        public string Id { get; set; }
        public List<CrawlerUnit> Units { get; set; } = new List<CrawlerUnit>();
        public int Range { get; set; }
        public long FactionTypeId { get; set; }

        public ECombatGroupActions CombatGroupAction { get; set; }

        public string SingularName { get; set; }
        public string PluralName { get; set; }
        public long UnitTypeId { get; set; }

        public CombatGroup()
        {
            Id = HashUtils.NewUUId();
        }
    }
}

using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Combat.Constants;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Units.Settings;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Combat.Entities
{
    public class CombatGroup
    {
        public string Id { get; set; }
        public List<CrawlerUnit> Units { get; set; } = new List<CrawlerUnit>();
        public int Range { get; set; }
        public long FactionTypeId { get; set; }

        public ECombatGroupActions CombatGroupAction { get; set; }

        public string SingularName { get; set; }
        public string PluralName { get; set; }
        public UnitType UnitType { get; set; }

        public CombatGroup()
        {
            Id = HashUtils.NewGuid();
        }
    }
}



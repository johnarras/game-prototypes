using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Inventory.PlayerData;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Spells.Entities
{
    /// <summary>
    /// Contains data about what this unit will do this round during combat.
    /// </summary>
    public class UnitAction
    {

        public string Text { get; set; }

        public CrawlerUnit Caster { get; set; }

        public List<CrawlerUnit> PossibleTargetUnits { get; set; } = new List<CrawlerUnit>();

        public List<CombatGroup> PossibleTargetGroups { get; set; } = new List<CombatGroup>();

        public List<CrawlerUnit> FinalTargets { get; set; } = new List<CrawlerUnit>();

        public List<CombatGroup> FinalTargetGroups { get; set; } = new List<CombatGroup>();

        public long CombatActionId { get; set; }

        public CrawlerSpell Spell { get; set; }

        public bool DidCast { get; set; }

        public bool NoCost { get; set; }

        public Item CastingItem { get; set; }

        public FullSpell SpellBeingCast { get; set; }

    }
}



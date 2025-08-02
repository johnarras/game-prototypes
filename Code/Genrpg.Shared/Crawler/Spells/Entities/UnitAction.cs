using MessagePack;
using Genrpg.Shared.Crawler.Monsters.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Inventory.PlayerData;

namespace Genrpg.Shared.Crawler.Spells.Entities
{
    /// <summary>
    /// Contains data about what this unit will do this round during combat.
    /// </summary>
    // MessagePackIgnore
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

        public bool IsComplete { get; set; }

        public Item CastingItem { get; set; }
    }
}

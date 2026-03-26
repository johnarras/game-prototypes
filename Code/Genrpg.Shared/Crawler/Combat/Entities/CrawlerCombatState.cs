using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Stats.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Combat.Entities
{

    public class InitialCombatState
    {
        public long Level { get; set; }
        public double Difficulty { get; set; } = 1.0f;
        public List<InitialCombatGroup> CombatGroups { get; set; } = new List<InitialCombatGroup>();
        public long WorldQuestItemId { get; set; }
    }


    public class InitialCombatGroup
    {
        public long UnitTypeId { get; set; }
        public long Quantity { get; set; }
        public int Range { get; set; }
        public long Level { get; set; }
        public string BossName { get; set; }
        public long FactionTypeId { get; set; }
    }


    public class CrawlerCombatState
    {
        public int RoundsComplete { get; set; } = 0;

        public long Level { get; set; } = 1;

        public List<CombatGroup> Enemies { get; set; } = new List<CombatGroup>();

        public List<CombatGroup> Allies { get; set; } = new List<CombatGroup>();

        public List<CrawlerUnit> EnemiesKilled { get; set; } = new List<CrawlerUnit>();

        public CombatGroup PartyGroup { get; set; }

        public List<StatVal> StatBuffs { get; set; } = new List<StatVal>();

        public List<CrawlerUnit> AttackSequence { get; set; } = new List<CrawlerUnit>();

        public bool PartyWonCombat() { return Enemies.Count == 0; }

        public double MaxDebuffTier { get; set; }

        public CombatGroup GetGroup(string combatGroupId)
        {
            CombatGroup group = Allies.FirstOrDefault(x => x.Id == combatGroupId);

            if (group == null)
            {
                group = Enemies.FirstOrDefault(x => x.Id == combatGroupId);
            }

            return group;
        }

        public List<CrawlerUnit> GetAllUnits()
        {
            List<CrawlerUnit> allUnits = new List<CrawlerUnit>();

            foreach (CombatGroup group in Allies)
            {
                allUnits.AddRange(group.Units);
            }

            foreach (CombatGroup group in Enemies)
            {
                allUnits.AddRange(group.Units);
            }

            return allUnits;
        }
    }
}



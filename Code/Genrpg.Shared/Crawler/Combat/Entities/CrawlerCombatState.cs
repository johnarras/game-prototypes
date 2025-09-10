using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Stats.Entities;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Combat.Entities
{

    [MessagePackObject]
    public class InitialCombatState
    {
        [Key(0)] public int Level { get; set; }
        [Key(1)] public double Difficulty { get; set; } = 1.0f;
        [Key(2)] public List<InitialCombatGroup> CombatGroups { get; set; } = new List<InitialCombatGroup>();
        [Key(3)] public long WorldQuestItemId { get; set; }
    }


    [MessagePackObject]
    public class InitialCombatGroup
    {
        [Key(0)] public long UnitTypeId { get; set; }
        [Key(1)] public long Quantity { get; set; }
        [Key(2)] public int Range { get; set; }
        [Key(4)] public int Level { get; set; }
        [Key(3)] public string BossName { get; set; }
        [Key(5)] public long FactionTypeId { get; set; }
    }


    // MessagePackIgnore
    public class CrawlerCombatState
    {
        public int RoundsComplete { get; set; } = 0;

        public int Level { get; set; } = 1;

        public List<CombatGroup> Enemies { get; set; } = new List<CombatGroup>();

        public List<CombatGroup> Allies { get; set; } = new List<CombatGroup>();

        public List<CrawlerUnit> EnemiesKilled { get; set; } = new List<CrawlerUnit>();

        public CombatGroup PartyGroup { get; set; }

        public List<StatVal> StatBuffs { get; set; } = new List<StatVal>();

        public long PlayerActionsRemaining { get; set; }

        public List<CrawlerUnit> AttackSequence { get; set; } = new List<CrawlerUnit>();

        public bool PartyWonCombat() { return Enemies.Count == 0; }


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

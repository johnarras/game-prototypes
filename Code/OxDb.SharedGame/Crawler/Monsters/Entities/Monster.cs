using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedGame.Crawler.Combat.Entities;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Monsters.Entities
{
    public class Monster : CrawlerUnit
    {
        public long MinDam { get; set; }
        public long MaxDam { get; set; }


        public SummonArgs SummonArgs { get; set; }

        public List<Effect> Spells { get; set; } = new List<Effect>();
        public List<FullEffect> ApplyEffects { get; set; } = new List<FullEffect>();

    }
}



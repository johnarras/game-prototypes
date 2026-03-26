using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Monsters.Entities
{
    public class Monster : CrawlerUnit
    {
        public long MinDam { get; set; }
        public long MaxDam { get; set; }

        public List<Effect> Spells { get; set; } = new List<Effect>();
        public List<FullEffect> ApplyEffects { get; set; } = new List<FullEffect>();

    }
}



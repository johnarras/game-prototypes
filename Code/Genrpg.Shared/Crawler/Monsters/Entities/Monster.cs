using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Monsters.Entities
{
    // MessagePackIgnore
    public class Monster : CrawlerUnit
    {
        public long MinDam { get; set; }
        public long MaxDam { get; set; }

        public List<UnitEffect> Spells { get; set; } = new List<UnitEffect>();
        public List<FullEffect> ApplyEffects { get; set; } = new List<FullEffect>();

    }
}

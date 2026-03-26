using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Inventory.PlayerData;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    public class FullSpell
    {
        public CrawlerSpell Spell { get; set; }
        public long HitQuantity { get; set; }
        public long HitsLeft { get; set; }
        public List<FullEffect> Effects { get; set; } = new List<FullEffect>();
        public long LuckyHitQuantity { get; set; }
        public Item CastingItem { get; set; }
    }
}



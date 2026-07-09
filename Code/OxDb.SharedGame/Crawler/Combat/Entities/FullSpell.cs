using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Inventory.PlayerData;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Combat.Entities
{
    public class FullSpell
    {
        public CrawlerSpell Spell { get; set; }
        public long HitQuantity { get; set; }
        public long HitsLeft { get; set; }
        public List<FullEffect> Effects { get; set; } = new List<FullEffect>();
        public long LuckyHitQuantity { get; set; }
        public Item CastingItem { get; set; }
        public bool HasLeadership { get; set; }
        public long RoleScalingTypeId { get; set; }
        public long StatScalingTypeId { get; set; }
    }
}



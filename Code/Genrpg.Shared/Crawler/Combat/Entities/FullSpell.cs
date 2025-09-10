using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Inventory.PlayerData;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    [MessagePackObject]
    public class FullSpell
    {
        [Key(0)] public CrawlerSpell Spell { get; set; }
        [Key(1)] public long HitQuantity { get; set; }
        [Key(2)] public long HitsLeft { get; set; }
        [Key(3)] public List<FullEffect> Effects { get; set; } = new List<FullEffect>();
        [Key(4)] public long LuckyHitQuantity { get; set; }
        [Key(5)] public Item CastingItem { get; set; }
    }
}

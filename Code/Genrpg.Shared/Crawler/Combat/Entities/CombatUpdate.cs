using Genrpg.Shared.Crawler.Monsters.Entities;
using MessagePack;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    public class CombatUpdate
    {
        [IgnoreMember] public CrawlerUnit Attacker { get; set; }
        [IgnoreMember] public CrawlerUnit Defender { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public long Quantity { get; set; }
    }
}



using MessagePack;
using Genrpg.Shared.Crawler.Monsters.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    // MessagePackIgnore
    public class CombatUpdate
    {
        [IgnoreMember] public CrawlerUnit Attacker { get; set; }
        [IgnoreMember] public CrawlerUnit Defender { get; set; }
        [Key(2)] public long EntityTypeId { get; set; }
        [Key(3)] public long EntityId { get; set; }
        [Key(4)] public long Quantity { get; set; }
    }
}

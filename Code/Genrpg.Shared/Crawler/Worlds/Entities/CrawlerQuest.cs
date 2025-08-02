using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Worlds.Entities
{
    [MessagePackObject]
    public class CrawlerQuest : IIdName
    { 
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public long CrawlerQuestTypeId { get; set; } 
        [Key(3)] public long TargetEntityId { get; set; } // Contextual based on the targettype id
        [Key(4)] public long Quantity { get; set; }
        [Key(5)] public long StartCrawlerNpcId { get; set; }
        [Key(6)] public long EndCrawlerNpcId { get; set; }
        [Key(7)] public long CrawlerMapId { get; set; }
        [Key(8)] public string TargetSingularName { get; set; }
        [Key(9)] public string TargetPluralName { get; set; }
    }
}

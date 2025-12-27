using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Worlds.Entities
{
    public class CrawlerQuest : IIdName
    { 
        public long IdKey { get; set; }
        public string Name { get; set; }
        public long CrawlerQuestTypeId { get; set; } 
        public long TargetEntityId { get; set; } // Contextual based on the targettype id
        public long Quantity { get; set; }
        public long StartCrawlerNpcId { get; set; }
        public long EndCrawlerNpcId { get; set; }
        public long CrawlerMapId { get; set; }
        public string TargetSingularName { get; set; }
        public string TargetPluralName { get; set; }
    }
}



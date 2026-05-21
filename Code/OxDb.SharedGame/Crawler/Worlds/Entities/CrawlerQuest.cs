using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;

namespace OxDb.SharedGame.Crawler.Worlds.Entities
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
        protected string _analyticsName = null;
        public string GetAnalyticsName()
        {
            if (string.IsNullOrEmpty(_analyticsName))
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    _analyticsName = StrUtils.ToSnakeCase(Name);
                }

                if (string.IsNullOrEmpty(_analyticsName))
                {
                    _analyticsName = StrUtils.ToSnakeCase(GetType().Name);
                }
            }
            return _analyticsName;
        }
    }
}



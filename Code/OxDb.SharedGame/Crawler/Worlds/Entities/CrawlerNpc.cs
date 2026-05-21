using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;

namespace OxDb.SharedGame.Crawler.Worlds.Entities
{
    public class CrawlerNpc : IIdName
    {
        public long IdKey { get; set; }
        public string Name { get; set; }
        public long UnitTypeId { get; set; }
        public long Level { get; set; }
        public long MapId { get; set; }
        public int X { get; set; }
        public int Z { get; set; }
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



using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;

namespace OxDb.SharedGame.Crawler.Worlds.Entities
{
    public class WorldQuestItem : IIdName
    {
        public long IdKey { get; set; }
        public string Name { get; set; }
        public long FoundInMapId { get; set; }
        public long UnlocksMapId { get; set; }
        public string GuardName { get; set; }
        public long GuardUnitTypeId { get; set; }
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



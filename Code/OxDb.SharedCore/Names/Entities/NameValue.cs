using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;

namespace OxDb.SharedCore.Names.Entities
{

    public class NameValue : IIdName
    {
        public long IdKey { get; set; }
        public string Name { get; set; }
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


    public class KeyValue
    {
        public string Key { get; set; }
        public string Val { get; set; }
    }



    public class NameIdValue : IIdName
    {
        public long IdKey { get; set; }
        public string Name { get; set; }
        public long Val { get; set; }
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



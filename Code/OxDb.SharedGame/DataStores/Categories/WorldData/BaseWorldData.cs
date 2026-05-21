using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;

namespace OxDb.SharedGame.DataStores.Categories.WorldData
{
    [DataGroup(EDataCategories.Worlds, ERepoTypes.Mongo)]
    public abstract class BaseWorldData : ISearchableItem, IName
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
        [MessagePack.IgnoreMember]
        public abstract string Name { get; set; }
        public abstract void Delete(IRepositoryService repoSystem);
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



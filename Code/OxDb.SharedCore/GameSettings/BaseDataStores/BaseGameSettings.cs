using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedCore.GameSettings.BaseDataStores
{
    [DataGroup(EDataCategories.Settings, ERepoTypes.Polymorphic)]
    public abstract class BaseGameSettings : IGameSettings
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
        [MessagePack.IgnoreMember]
        public virtual string Name
        {
            get { return GetType().Name; }
            set { }
        }
        [MessagePack.IgnoreMember]
        public DateTime SaveTime { get; set; } = DateTime.MinValue;

        public virtual void SetInternalIds() { }
        public virtual void ClearIndex() { }

        public virtual async Task SaveAll(IRepositoryService repo)
        {
            await repo.Save(this);
        }
        public virtual List<IGameSettings> GetChildren() { return new List<IGameSettings>(); }
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



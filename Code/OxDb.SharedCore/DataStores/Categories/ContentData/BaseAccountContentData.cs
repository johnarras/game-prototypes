using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Interfaces;

namespace OxDb.SharedCore.DataStores.Categories.ContentData
{
    [DataGroup(EDataCategories.Accounts, ERepoTypes.Blob)]
    public abstract class BaseAccountContentData : IStringId
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
    }
}



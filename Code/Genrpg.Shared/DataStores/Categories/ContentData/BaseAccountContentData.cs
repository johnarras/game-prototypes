using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.DataStores.Categories.ContentData
{
    [DataGroup(EDataCategories.Accounts, ERepoTypes.Blob)]

    public abstract class BaseAccountContentData : IStringId
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
    }
}



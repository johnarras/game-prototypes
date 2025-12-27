using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.DataStores.Categories.ContentData
{
    [DataGroup(EDataCategories.Settings, ERepoTypes.Blob)]

    public abstract class BaseGameContentData : IStringId
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
    }
}



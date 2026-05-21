using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Interfaces;

namespace OxDb.SharedGame.DataStores.Categories.ContentData
{
    [DataGroup(EDataCategories.Players, ERepoTypes.Blob)]

    public abstract class BasePublicPlayerData : IStringId
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
    }
}



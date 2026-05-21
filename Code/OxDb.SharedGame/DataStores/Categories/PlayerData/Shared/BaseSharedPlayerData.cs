using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Interfaces;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.Shared
{
    [DataGroup(EDataCategories.Players, ERepoTypes.Mongo)]
    public abstract class BaseSharedPlayerData : IStringId
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
    }
}



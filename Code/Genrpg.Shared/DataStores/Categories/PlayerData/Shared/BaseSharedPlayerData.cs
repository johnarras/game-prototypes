using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.Shared
{
    [DataGroup(EDataCategories.Players, ERepoTypes.Mongo)]
    public abstract class BaseSharedPlayerData : IStringId
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
    }
}



using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.Shared
{
    [DataGroup(EDataCategories.Players, ERepoTypes.NoSQL)]
    public abstract class BaseSharedPlayerData : IStringId
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
    }
}

using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using MessagePack;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.Core
{
    [DataGroup(EDataCategories.Players, ERepoTypes.NoSQL)]
    // MessagePackIgnore
    public abstract class BasePlayerData : IUnitData
    {
        [IgnoreMember]
        public abstract string Id { get; set; }

        public abstract IUnitData Unpack();

    }
}

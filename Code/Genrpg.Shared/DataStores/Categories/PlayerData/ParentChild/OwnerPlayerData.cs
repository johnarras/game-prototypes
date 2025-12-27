using Genrpg.Shared.DataStores.Categories.PlayerData.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild
{
    [DataGroup(EDataCategories.Players, ERepoTypes.Mongo)]
    public abstract class OwnerPlayerData : BasePlayerData, IStringOwnerId, IChildUnitData
    {
        [MessagePack.IgnoreMember]
        public abstract string OwnerId { get; set; }

        public override IUnitData Unpack() { return this; }
    }
}



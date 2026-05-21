using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Core;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Interfaces;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild
{
    [DataGroup(EDataCategories.Players, ERepoTypes.Mongo)]
    public abstract class OwnerPlayerData : BasePlayerData, IStringOwnerId, IChildUnitData, ISearchableItem
    {
        [MessagePack.IgnoreMember]
        public abstract string OwnerId { get; set; }

        public override IUnitData Unpack() { return this; }
    }
}



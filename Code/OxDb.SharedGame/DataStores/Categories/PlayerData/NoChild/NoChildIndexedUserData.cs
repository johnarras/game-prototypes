using MessagePack;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.DataStores.Interfaces;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild
{
    [DataGroup(EDataCategories.Players, ERepoTypes.Mongo)]
    public abstract class NoChildIndexedUserData : VersionedNoChildPlayerData, ISearchableItem
    {
    }
}

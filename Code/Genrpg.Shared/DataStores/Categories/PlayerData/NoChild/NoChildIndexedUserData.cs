using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Interfaces;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.NoChild
{
    [DataGroup(EDataCategories.Players, ERepoTypes.Mongo)]
    public abstract class NoChildIndexedUserData : NoChildPlayerData, ISearchableItem
    {
    }
}

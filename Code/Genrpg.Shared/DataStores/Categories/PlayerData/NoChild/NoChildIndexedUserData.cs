using Genrpg.Shared.DataStores.DataGroups;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.NoChild
{
    [DataGroup(EDataCategories.Players, ERepoTypes.Mongo)]
    public abstract class NoChildIndexedUserData : NoChildPlayerData
    {
    }
}

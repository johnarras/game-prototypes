using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Accounts.PlayerData
{
    [DataGroup(EDataCategories.Accounts, ERepoTypes.Mongo)]
    public abstract class BaseAccountData : ISearchableItem
    {
        [IgnoreMember] public abstract string Id { get; set; }
    }
}



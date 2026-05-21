using MessagePack;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.DataStores.Interfaces;

namespace OxDb.PlatformServer.Accounts.PlayerData
{
    [DataGroup(EDataCategories.Accounts, ERepoTypes.Mongo)]
    public abstract class BaseAccountData : ISearchableItem
    {
        [IgnoreMember] public abstract string Id { get; set; }
    }
}



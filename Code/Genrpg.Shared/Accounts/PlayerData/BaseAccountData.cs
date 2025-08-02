using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Accounts.PlayerData
{
    [DataGroup(EDataCategories.Accounts,ERepoTypes.NoSQL)]
    public abstract class BaseAccountData : IStringId
    {
        [IgnoreMember] public abstract string Id { get; set; }
    }
}

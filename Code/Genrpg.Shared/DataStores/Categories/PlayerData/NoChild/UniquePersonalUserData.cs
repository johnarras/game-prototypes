using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Utils;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.NoChild
{
    [DataGroup(EDataCategories.Players, ERepoTypes.NoSQL)]
    public abstract class UniquePersonalUserData : NoChildPlayerData, IUniquePersonalUserData
    {

        private string _pk = null;
        [MessagePack.IgnoreMember]
        public string pk
        {
            get
            {
                if (string.IsNullOrEmpty(_pk))
                {
                    if (!string.IsNullOrEmpty(Id))
                    {
                        string justUserId = Id.Replace(NoSqlUtils.GetDocIdSuffix(GetType()), "");
                        if (!string.IsNullOrWhiteSpace(justUserId))
                        {
                            _pk = justUserId;
                        }
                    }
                }
                return _pk;
            }
        }

        [MessagePack.IgnoreMember]
        public string _etag { get; set; }
    }
}

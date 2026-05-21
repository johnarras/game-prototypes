using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;
using OxDb.SharedGame.DataStores.Utils;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild
{
    [DataGroup(EDataCategories.Players, ERepoTypes.NoSQL)]
    public abstract class UniquePersonalUserData : NoChildPlayerData, IUniquePersonalUserData
    {

        public abstract int GetOffsetBit();
        public abstract PersonalDataAccumulation GetAccumulation();
        public virtual bool WasEverSaved() { return !string.IsNullOrEmpty(_etag); }

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

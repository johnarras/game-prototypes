using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Core;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;
using OxDb.SharedGame.DataStores.Utils;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild
{
    [DataGroup(EDataCategories.Players, ERepoTypes.NoSQL)]
    public abstract class UniquePersonalUserData : PartitionedNoChildPlayerData, IUniquePersonalUserData
    {

        public abstract int GetOffsetBit();
        public abstract PersonalDataAccumulation GetAccumulation();
        public virtual bool WasEverSaved() { return !string.IsNullOrEmpty(_etag); }

    }
}

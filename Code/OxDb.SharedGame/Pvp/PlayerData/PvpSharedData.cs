using OxDb.SharedGame.DataStores.Categories.PlayerData.Shared;
using OxDb.SharedGame.Users.Loaders;

namespace OxDb.SharedGame.Pvp.PlayerData
{
    public class PvpSharedData : BaseSharedPlayerData
    {
        public override string Id { get; set; }

        // Which tile indexes are damaged
        public long Damage { get; set; }

        // Which tile indexes have guards
        public long Guards { get; set; }
    }


    public class PvpSharedDataLoader : SharedUserDataLoader<PvpSharedData> { }
}



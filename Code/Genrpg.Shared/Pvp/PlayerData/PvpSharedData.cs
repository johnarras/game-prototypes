using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData.Shared;
using Genrpg.Shared.Users.Loaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Pvp.PlayerData
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



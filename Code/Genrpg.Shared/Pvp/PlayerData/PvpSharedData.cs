using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData.Shared;
using Genrpg.Shared.Users.Loaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Pvp.PlayerData
{
    [MessagePackObject]
    public class PvpSharedData : BaseSharedPlayerData
    {
        [Key(0)] public override string Id { get; set; }

        // Which tile indexes are damaged
        [Key(1)] public long Damage { get; set; }

        // Which tile indexes have guards
        [Key(2)] public long Guards { get; set; }
    }


    public class PvpSharedDataLoader : SharedUserDataLoader<PvpSharedData> { }
}

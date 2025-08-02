using Genrpg.Shared.DataStores.Categories.PlayerData.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using MessagePack;
using System;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.NoChild
{
    public abstract class NoChildPlayerData : BasePlayerData, ITopLevelUnitData
    {
        public override IUnitData Unpack() { return this; }
        [IgnoreMember] public DateTime UpdateTime { get; set; }
    }
}

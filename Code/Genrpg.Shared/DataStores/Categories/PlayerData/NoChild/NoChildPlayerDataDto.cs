using Genrpg.Shared.DataStores.Categories.PlayerData.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.NoChild
{
    public abstract class NoChildPlayerDataDto<TPlayerData> : StubUnitData where TPlayerData : NoChildPlayerData
    {
        [MessagePack.IgnoreMember] public abstract TPlayerData Parent { get; set; }

        public override IUnitData Unpack() { return Parent; }
    }
}



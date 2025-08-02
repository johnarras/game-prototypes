using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.NoChild
{
    [MessagePackObject]
    public class NoChildPlayerDataDto<TPlayerData> : StubUnitData where TPlayerData : NoChildPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public TPlayerData Parent { get; set; }

        public override IUnitData Unpack() { return Parent; }
    }
}

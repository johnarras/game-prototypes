using OxDb.SharedGame.DataStores.Categories.PlayerData.Core;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild
{
    public abstract class NoChildPlayerDataDto<TPlayerData> : StubUnitData where TPlayerData : NoChildPlayerData
    {
        [MessagePack.IgnoreMember] public abstract TPlayerData Parent { get; set; }

        public override IUnitData Unpack() { return Parent; }
    }
}



using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Utils.Data;
using MessagePack;

namespace Genrpg.Shared.Trader.Holdings.PlayerData
{

    /// <summary>
    /// This is modified only when the player buys or sells items or adds or removes animals.
    /// </summary>
    [MessagePackObject]
    public class HoldingsData : NoChildPlayerData, IUserData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public SmallIndexBitList AnimalsOwned { get; set; } = new SmallIndexBitList();

        [Key(3)] public SmallIndexBitList CityWarehouses { get; set; } = new SmallIndexBitList();
    }
}

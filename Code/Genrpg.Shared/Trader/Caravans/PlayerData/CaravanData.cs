using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Caravans.PlayerData
{

    /// <summary>
    /// This is modified only when the player buys or sells items or adds or removes animals.
    /// </summary>
    [MessagePackObject]
    public class CaravanData : UniquePersonalUserData, IUserData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public List<CaravanAnimal> Animals { get; set; } = new List<CaravanAnimal>();

        [Key(2)] public List<CaravanTradeGood> TradeGoods { get; set; } = new List<CaravanTradeGood>();
    }



    [MessagePackObject]
    public class CaravanAnimal
    {
        [Key(0)] public long AnimalTypeId { get; set; }
        [Key(1)] public long SkinTypeId { get; set; }
    }

    [MessagePackObject]
    public class CaravanTradeGood
    {
        [Key(0)] public long TradeGoodId { get; set; }
    }
}



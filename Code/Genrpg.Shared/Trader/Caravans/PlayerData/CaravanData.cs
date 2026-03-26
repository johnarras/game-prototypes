using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Caravans.PlayerData
{

    /// <summary>
    /// This is modified only when the player buys or sells items or adds or removes CaravanMembers.
    /// </summary>
    [MessagePackObject]
    public class CaravanData : UniquePersonalUserData, IUserData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public List<CurrentCaravanMember> CurrentMembers { get; set; } = new List<CurrentCaravanMember>();

        [Key(2)] public List<CaravanTradeGood> TradeGoods { get; set; } = new List<CaravanTradeGood>();

        [Key(3)] public long SkinTypeId { get; set; }
    }



    [MessagePackObject]
    public class CurrentCaravanMember
    {
        [Key(0)] public long CaravanMemberId { get; set; }
        [Key(1)] public long SkinTypeId { get; set; }
    }

    [MessagePackObject]
    public class CaravanTradeGood
    {
        [Key(0)] public long TradeGoodId { get; set; }
    }


    public class CaravanDataLoader : UnitDataLoader<CaravanData> { }


    [MessagePackObject]
    public class CaravanDto : NoChildPlayerDataDto<CaravanData>
    {
        [Key(0)] public override CaravanData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class CaravanDataMapper : NoChildUnitDataMapper<CaravanData, CaravanDto> { }
}



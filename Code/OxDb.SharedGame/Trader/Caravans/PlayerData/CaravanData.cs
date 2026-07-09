using MessagePack;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.Caravans.PlayerData
{

    /// <summary>
    /// This is modified only when the player buys or sells items or adds or removes CaravanMembers.
    /// </summary>
    [MessagePackObject]
    public class CaravanData : UniquePersonalUserData, IUserData
    {



        public override int GetOffsetBit() { return PersonalDataOffsetBits.Caravan; }
        public override PersonalDataAccumulation GetAccumulation()
        {
            return new PersonalDataAccumulation();
        }

        [Key(0)] public override string Id { get; set; }

        [Key(1)] public List<CurrentCaravanMember> CurrentMembers { get; set; } = new List<CurrentCaravanMember>();

        [Key(2)] public List<CaravanTradeGood> TradeGoods { get; set; } = new List<CaravanTradeGood>();

        [Key(3)] public long SkinTypeId { get; set; }
    }



    [MessagePackObject]
    public class CurrentCaravanMember
    {
        [Key(0)] public long CaravanMemberId { get; set; }
    }

    [MessagePackObject]
    public class CaravanTradeGood
    {
        [Key(0)] public long TradeGoodId { get; set; }
        [Key(1)] public long UniqueId { get; set; }
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



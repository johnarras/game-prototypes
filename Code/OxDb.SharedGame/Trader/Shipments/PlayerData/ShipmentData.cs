using MessagePack;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.Shipments.PlayerData
{

    /// <summary>
    /// This is modified only when the player buys or sells items or adds or removes ShipmentMembers.
    /// </summary>
    [MessagePackObject]
    public class ShipmentData : UniquePersonalUserData, IUserData
    {
        public override int GetOffsetBit() { return EPersonalDataOffsetBits.Shipments; }

        public override PersonalDataAccumulation GetAccumulation()
        {
            PersonalDataAccumulation accumulation = new PersonalDataAccumulation()
            {
            };

            return accumulation;
        }

        [Key(0)] public override string Id { get; set; }

        [Key(1)] public List<Shipment> Shipments { get; set; } = new List<Shipment>();
    }


    [MessagePackObject]
    public class Shipment
    {
        [Key(0)] public long UniqueId { get; set; }
        [Key(1)] public long BonusDueDay { get; set; }
        [Key(2)] public List<Reward> Rewards { get; set; } = new List<Reward>();
        [Key(3)] public List<Reward> BonusRewards { get; set; } = new List<Reward>();
        [Key(4)] public List<ShipmentTask> Tasks { get; set; } = new List<ShipmentTask>();
    }

    [MessagePackObject]
    public class ShipmentTask
    {
        /// <summary>
        /// What trade good we need to get.
        /// </summary>
        [Key(0)] public long TradeGoodId { get; set; }
        /// <summary>
        /// Where we get it.
        /// </summary>
        [Key(1)] public long CityId { get; set; }
        /// <summary>
        /// What UniqueId it ends up having.
        /// </summary>
        [Key(2)] public long UniqueId { get; set; }
    }


    public class ShipmentDataLoader : UnitDataLoader<ShipmentData> { }


    [MessagePackObject]
    public class ShipmentDto : NoChildPlayerDataDto<ShipmentData>
    {
        [Key(0)] public override ShipmentData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class ShipmentDataMapper : NoChildUnitDataMapper<ShipmentData, ShipmentDto> { }
}



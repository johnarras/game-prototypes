using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Shipments.PlayerData
{

    /// <summary>
    /// This is modified only when the player buys or sells items or adds or removes ShipmentMembers.
    /// </summary>
    [MessagePackObject]
    public class ShipmentData : UniquePersonalUserData, IUserData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public List<Shipment> Shipments { get; set; } = new List<Shipment>();
    }


    [MessagePackObject]
    public class ShipmentTask
    {
        [Key(0)] public long TradeGoodId { get; set; }
        [Key(1)] public long CityId { get; set; }
        [Key(2)] public long Quantity { get; set; }
    }

    [MessagePackObject]
    public class Shipment
    {
        [Key(0)] public long ShipmentId { get; set; }
        [Key(1)] public long BonusDueDay { get; set; }
        [Key(2)] public List<Reward> Rewards { get; set; } = new List<Reward>();
        [Key(3)] public List<Reward> BonusRewards { get; set; } = new List<Reward>();

        [Key(4)] public List<ShipmentTask> Tasks { get; set; } = new List<ShipmentTask>();

        
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



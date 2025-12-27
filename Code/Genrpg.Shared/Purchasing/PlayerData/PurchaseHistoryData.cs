using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using MessagePack;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.PlayerData
{

    public class PurchaseHistoryLoader : UnitDataLoader<PurchaseHistoryData> { }

    [MessagePackObject]
    public class PurchaseHistoryDto : NoChildPlayerDataDto<PurchaseHistoryData>
    {
        [Key(0)] public override PurchaseHistoryData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class PurchaseHistoryDataMapper : NoChildUnitDataMapper<PurchaseHistoryData, PurchaseHistoryDto> { }

    [MessagePackObject]
    public class PurchaseHistoryData : UniquePersonalUserData, IUserData, IServerOnlyData
    {
        public const int MaxRecentPurchasesCount = 10;

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double PurchaseTotal { get; set; }
        [Key(2)] public long PurchaseCount { get; set; }
        [Key(3)] public DateTime FirstPurchase { get; set; }
        [Key(4)] public DateTime LatestPurchase { get; set; }

        [Key(5)] public List<RecentPurchase> RecentPurchases { get; set; } = new List<RecentPurchase>();
    }

    [MessagePackObject]
    public class RecentPurchase
    {
        [Key(0)] public DateTime PurchaseTime { get; set; }
        [Key(1)] public double Price { get; set; }
        [Key(2)] public long ProductSkuId { get; set; }
        [Key(3)] public long StoreItemId { get; set; }
        [Key(4)] public long StoreSlotId { get; set; }
        [Key(5)] public string Name { get; set; }
    }
}



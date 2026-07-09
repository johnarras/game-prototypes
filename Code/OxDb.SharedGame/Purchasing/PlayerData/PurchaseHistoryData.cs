using MessagePack;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System;
using System.Collections.Generic;

namespace OxDb.SharedGame.Purchasing.PlayerData
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
        public override int GetOffsetBit() { return PersonalDataOffsetBits.PurchaseHistory; }
        public override PersonalDataAccumulation GetAccumulation()
        {
            return new PersonalDataAccumulation();
        }
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



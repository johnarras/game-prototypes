using MessagePack;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;
using OxDb.SharedGame.Purchasing.Constants;
using OxDb.SharedGame.Units.Loaders;
using System.Collections.Generic;

namespace OxDb.SharedGame.Purchasing.PlayerData
{


    [MessagePackObject]
    public class CurrentPurchaseData : UniquePersonalUserData, IUserData, IServerOnlyData
    {

        public override int GetOffsetBit() { return PersonalDataOffsetBits.CurrentPurchases; }

        public override PersonalDataAccumulation GetAccumulation() { return new PersonalDataAccumulation(); }

        [Key(0)] public override string Id { get; set; }

        [Key(1)] public string OfferId { get; set; }

        [Key(2)] public string BundleId { get; set; }

        [Key(3)] public string UniqueId { get; set; }

        [Key(4)] public string ReceiptData { get; set; }

        [Key(5)] public string ProductId { get; set; }

        [Key(6)] public List<Reward> Rewards { get; set; } = new List<Reward>();

        [Key(7)] public EPurchasePlatforms Platform { get; set; }

        [Key(8)] public ECurrentPurchaseStates State { get; set; }

        [Key(9)] public int FailedValidationTimes { get; set; }


        public bool HasFullOrder()
        {
            return !string.IsNullOrEmpty(OfferId) &&
                !string.IsNullOrEmpty(BundleId) &&
                !string.IsNullOrEmpty(UniqueId) &&
                !string.IsNullOrWhiteSpace(ProductId) &&
                !string.IsNullOrEmpty(ReceiptData);

        }

        public void Clear()
        {
            OfferId = null;
            BundleId = null;
            UniqueId = null;
            ReceiptData = null;
            ProductId = null;
            Rewards = new List<Reward>();
            Platform = EPurchasePlatforms.Editor;
            State = ECurrentPurchaseStates.None;
            FailedValidationTimes = 0;
        }
    }

    public class CurrentPurchaseLoader : UnitDataLoader<CurrentPurchaseData> { }
}



using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Units.Loaders;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.PlayerData
{


    [MessagePackObject]
    public class CurrentPurchaseData : NoChildPlayerData, IUserData, IServerOnlyData
    {

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

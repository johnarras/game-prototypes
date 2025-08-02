using MessagePack;
using Genrpg.Shared.Characters.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.DataStores.Categories.PlayerData.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;

namespace Genrpg.Shared.Purchasing.PlayerData
{
    [MessagePackObject]
    public class CompletedPurchaseData : NoChildPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string UserId { get; set; }
        [Key(2)] public DateTime Date { get; set; }
        [Key(3)] public string ReceiptHash { get; set; }
        [Key(4)] public string ReceiptData { get; set; }
    }
}

using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Website.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Purchasing.WebApi.ValidatePurchase
{
    [MessagePackObject]
    public class ValidatePurchaseRequest : IClientUserRequest
    {
        [Key(0)] public string OfferId { get; set; }
        [Key(1)] public string BundleId { get; set; }
        [Key(2)] public string UniqueId { get; set; }
        [Key(3)] public string ReceiptData { get; set; }
        [Key(5)] public EPurchasePlatforms Platform { get; set; }

    }
}

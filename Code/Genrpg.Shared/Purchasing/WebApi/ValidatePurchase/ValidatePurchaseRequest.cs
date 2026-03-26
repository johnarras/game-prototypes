using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Purchasing.WebApi.ValidatePurchase
{
    public class ValidatePurchaseRequest : IClientUserRequest
    {
        public string OfferId { get; set; }
        public string BundleId { get; set; }
        public string UniqueId { get; set; }
        public string ReceiptData { get; set; }
        public EPurchasePlatforms Platform { get; set; }

    }
}



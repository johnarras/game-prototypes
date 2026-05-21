using OxDb.SharedCore.Website.Interfaces;
using OxDb.SharedGame.Purchasing.Constants;

namespace OxDb.SharedGame.Purchasing.WebApi.ValidatePurchase
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



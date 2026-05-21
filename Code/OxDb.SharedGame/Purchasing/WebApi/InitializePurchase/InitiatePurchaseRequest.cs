using OxDb.SharedCore.Website.Interfaces;
using OxDb.SharedGame.Purchasing.Constants;

namespace OxDb.SharedGame.Purchasing.WebApi.InitializePurchase
{
    public class InitiatePurchaseRequest : IClientUserRequest
    {
        public string OfferId { get; set; }
        public string UniqueId { get; set; }
        public string BundleId { get; set; }
        public EPurchasePlatforms Platform { get; set; }
    }
}



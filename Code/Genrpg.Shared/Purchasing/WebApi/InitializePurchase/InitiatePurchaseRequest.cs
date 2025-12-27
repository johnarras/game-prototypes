using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Website.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Purchasing.WebApi.InitializePurchase
{
    public class InitiatePurchaseRequest : IClientUserRequest
    {
        public string OfferId { get; set; }
        public string UniqueId { get; set; }
        public string BundleId { get; set; }
        public EPurchasePlatforms Platform { get; set; }
    }
}



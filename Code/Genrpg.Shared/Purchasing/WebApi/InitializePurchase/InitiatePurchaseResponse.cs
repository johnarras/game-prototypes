using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Purchasing.WebApi.InitializePurchase
{
    public class InitiatePurchaseResponse : IWebResponse
    {
        public EInitiatePurchaseStates State { get; set; }
        public string OfferId { get; set; }
        public string UniqueId { get; set; }
        public string BundleId { get; set; }
        public string ProductId { get; set; }
    }
}



using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.Purchasing.Constants;

namespace OxDb.SharedGame.Purchasing.WebApi.InitializePurchase
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



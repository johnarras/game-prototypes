using OxDb.RequestServer.Purchasing.Entities;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Purchasing.Constants;

namespace OxDb.RequestServer.Purchasing.ValidationHelpers
{
    public interface IPurchaseValidationHelper : ISetupDictionaryItem<EPurchasePlatforms>, IInitializable
    {
        Task<PurchaseValidationResult> ValidatePurchase(string productId, string receiptData);
    }
}



using OxDb.RequestServer.Purchasing.Entities;
using OxDb.SharedGame.Purchasing.Constants;

namespace OxDb.RequestServer.Purchasing.ValidationHelpers
{
    public class EditorPurchaseValidationHelper : IPurchaseValidationHelper
    {
        public EPurchasePlatforms HelperKey => EPurchasePlatforms.Editor;

        public Task Initialize(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task<PurchaseValidationResult> ValidatePurchase(string productId, string uniquePurchaseId)
        {
            await Task.CompletedTask;

            return new PurchaseValidationResult()
            {
                State = EPurchaseValidationStates.Success,
            };


        }
    }
}



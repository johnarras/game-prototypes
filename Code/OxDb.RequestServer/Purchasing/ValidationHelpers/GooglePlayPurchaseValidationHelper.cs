using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using OxDb.RequestServer.Purchasing.Entities;
using OxDb.ServerCore.Config;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedGame.Purchasing.Constants;

namespace OxDb.RequestServer.Purchasing.ValidationHelpers
{
    public class GooglePlayPurchaseValidationHelper : IPurchaseValidationHelper
    {
        private IServerConfig _serverConfig = null;


        public EPurchasePlatforms HelperKey => EPurchasePlatforms.GooglePlay;

        private string _packageName;
        private AndroidPublisherService _publisherService;
        public async Task Initialize(CancellationToken token)
        {
            _packageName = _serverConfig.GetConfigVal(AppConfigKeys.PackageName);

            string jsonSecret = _serverConfig.GetConfigVal(AppConfigKeys.GooglePlayPurchasingSecret);

            GoogleCredential credential = GoogleCredential
                .FromJson(jsonSecret)
                .CreateScoped("https://www.googleapis.com/auth/androidpublisher");


            _publisherService = new AndroidPublisherService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _serverConfig.ProductName,
            });
            await Task.CompletedTask;
        }

        public async Task<PurchaseValidationResult> ValidatePurchase(string productId, string purchaseToken)
        {

            try
            {
                Google.Apis.AndroidPublisher.v3.Data.ProductPurchase purchase = await _publisherService.Purchases.Products.Get(_packageName, productId, purchaseToken).ExecuteAsync();
                if (purchase.PurchaseState == 0)
                {
                    return new PurchaseValidationResult()
                    {
                        State = EPurchaseValidationStates.Success,
                    };
                }
                else
                {
                    PurchaseValidationResult result = new PurchaseValidationResult()
                    {
                        State = EPurchaseValidationStates.Failed,
                        ErrorMessage = $"Validation failed with purchase state: {purchase.PurchaseState}"
                    };
                    return result;
                }
            }
            catch (Google.GoogleApiException ex)
            {
                PurchaseValidationResult result = new PurchaseValidationResult()
                {
                    State = EPurchaseValidationStates.Failed,
                    ErrorMessage = $"Google API error: {ex.Message}"
                };

                return result;
            }
            catch (Exception ex)
            {
                PurchaseValidationResult result = new PurchaseValidationResult()
                {
                    State = EPurchaseValidationStates.Failed,
                    ErrorMessage = $"Unexpected error: {ex.Message}"
                };
                return result;
            }
        }
    }
}


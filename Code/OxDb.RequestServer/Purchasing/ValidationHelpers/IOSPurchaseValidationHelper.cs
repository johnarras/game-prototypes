using OxDb.RequestServer.Purchasing.Entities;
using OxDb.ServerCore.Config;
using OxDb.ServerCore.WebRequests.Services;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Purchasing.Constants;


namespace OxDb.RequestServer.Purchasing.ValidationHelpers
{
    public class IOSValidationResponse
    {
        public int Status { get; set; }
        public string Environment { get; set; }
        public IOSReceipt Receipt { get; set; }
        public List<object> LatestReceiptInfo { get; set; }
        public string LatestReceipt { get; set; }
        public List<object> PendingRenewalInfo { get; set; }
    }

    public class IOSReceipt
    {
        public string ReceiptType { get; set; }
        public int AdamId { get; set; }
        public int AppItemId { get; set; }
        public string BundleId { get; set; }
        public string ApplicationVersion { get; set; }
        public int DownloadId { get; set; }
        public int VersionExternalIdentifier { get; set; }
        public string ReceiptCreationDate { get; set; }
        public string ReceiptCreationDateMs { get; set; }
        public string ReceiptCreationDatePst { get; set; }
        public string RequestDate { get; set; }
        public string RequestDateMs { get; set; }
        public string RequestDatePst { get; set; }
        public string OriginalPurchaseDate { get; set; }
        public string OriginalPurchaseDateMs { get; set; }
        public string OriginalPurchaseDatePst { get; set; }
        public string OriginalApplicationVersion { get; set; }
        public List<IOSInApp> InApp { get; set; }
    }

    public class IOSInApp
    {
        public string Quantity { get; set; }
        public string ProductId { get; set; }
        public string TransactionId { get; set; }
        public string OriginalTransactionId { get; set; }
        public string PurchaseDate { get; set; }
        public string PurchaseDateMs { get; set; }
        public string PurchaseDatePst { get; set; }
        public string OriginalPurchaseDate { get; set; }
        public string OriginalPurchaseDateMs { get; set; }
        public string OriginalPurchaseDatePst { get; set; }
        public string IsTrialPeriod { get; set; }
        public string IsInIntroOfferPeriod { get; set; }
    }


    public class IOSPurchaseValidationHelper : IPurchaseValidationHelper
    {

        private IServerConfig _serverConfig = null;
        private ITextSerializer _serializer = null;
        private IWebRequestService _webRequestSevice = null;

        public EPurchasePlatforms HelperKey => EPurchasePlatforms.IOS;

        const int NoSandboxInProdStatus = 21007;


        private ReadOnlyString _iosSecret;
        private string _buyURL;
        private string _sandboxURL;
        public async Task Initialize(CancellationToken token)
        {
            _iosSecret = new ReadOnlyString(_serverConfig.GetConfigVal(AppConfigKeys.IOSSecret));
            _buyURL = _serverConfig.GetConfigVal(AppConfigKeys.IOSBuyValidationURL);
            _sandboxURL = _serverConfig.GetConfigVal(AppConfigKeys.IOSSandboxValidationURL);
            await Task.CompletedTask;
        }


        private bool _sandboxFailed = false;
        public async Task<PurchaseValidationResult> ValidatePurchase(string productId, string receiptData)
        {

            PurchaseValidationResult result = null;
            using (HttpClient client = new HttpClient())
            {
                Dictionary<string, object> requestPayload =
                    new Dictionary<string, object>()
                    {
                    { "password", _iosSecret.GetString()},
                    { "receipt-data", receiptData },
                    { "exclude-old-transactions", true }
                    };

                StringContent content = new StringContent(_serializer.SerializeToString(requestPayload));

                if (!_sandboxFailed)
                {
                    result = await CheckReceiptVsEndpoint(client, _sandboxURL, content);
                }

                if (_sandboxFailed || result == null || result.Status == NoSandboxInProdStatus)
                {
                    _sandboxFailed = true;
                    result = await CheckReceiptVsEndpoint(client, _buyURL, content);
                }

                return result;
            }
        }

        private async Task<PurchaseValidationResult> CheckReceiptVsEndpoint(HttpClient client, string endpoint, StringContent content)
        {
            HttpResponseMessage httpResponse = await client.PostAsync(endpoint, content);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception("HTTP request failed with status code: " + httpResponse.StatusCode);
            }

            string responseString = await httpResponse.Content.ReadAsStringAsync();

            IOSValidationResponse validationResponse = _serializer.Deserialize<IOSValidationResponse>(responseString);

            int status = validationResponse.Status;

            if (status == 0)
            {
                return new PurchaseValidationResult() { State = EPurchaseValidationStates.Success };
            }

            PurchaseValidationResult result = new PurchaseValidationResult()
            {
                ErrorMessage = $"Validation failed with status: {status}",
                State = EPurchaseValidationStates.Failed,
                Status = status,
            };

            return result;
        }
    }
}


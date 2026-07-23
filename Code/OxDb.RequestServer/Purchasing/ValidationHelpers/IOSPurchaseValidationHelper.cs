using OxDb.RequestServer.Purchasing.Entities;
using OxDb.ServerCore.Config;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.WebRequests.Services;
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
        private CancellationToken _token = default;
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


            Dictionary<string, object> postData = new Dictionary<string, object>()
                    {
                    { "password", _iosSecret.GetString()},
                    { "receipt-data", receiptData },
                    { "exclude-old-transactions", true }
            };
            if (!_sandboxFailed)
            {
                result = await CheckReceiptVsEndpoint(_sandboxURL, postData);
            }

            if (_sandboxFailed || result == null || result.Status == NoSandboxInProdStatus)
            {
                _sandboxFailed = true;
                result = await CheckReceiptVsEndpoint(_buyURL,postData);
            }

            return result;
        }

        private async Task<PurchaseValidationResult> CheckReceiptVsEndpoint(string endpoint, Dictionary<string,object> postData)
        {

            WebRequestOptions opts = new WebRequestOptions()
            {
                ContentType = HttpContentType.Json,
                JsonBody = postData,
                Method = HttpMethodType.Post,
            };

            ResponseEnvelope<IOSValidationResponse> validationEnvelope = await _webRequestSevice.SendAsync<IOSValidationResponse>(endpoint, opts, _token);
                
            if (!validationEnvelope.Success || validationEnvelope.Response == null || validationEnvelope.Response.Status != 0)
            {
                int status = validationEnvelope?.Response.Status ?? -1;
                PurchaseValidationResult result = new PurchaseValidationResult()
                {
                    ErrorMessage = $"Validation failed with status: {status} -- {validationEnvelope.ErrorMessage} ",
                    State = EPurchaseValidationStates.Failed,
                    Status = status,
                };
            }
            return new PurchaseValidationResult() { State = EPurchaseValidationStates.Success };
        }
    }
}


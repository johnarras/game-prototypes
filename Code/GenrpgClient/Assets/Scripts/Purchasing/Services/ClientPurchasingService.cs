using Genrpg.Shared.Client.Contants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Purchasing.WebApi.InitializePurchase;
using Genrpg.Shared.Purchasing.WebApi.ValidatePurchase;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Assets.Scripts.Purchasing.Services
{

    public enum EPurchasingInitializationState
    {
        NotInitialized = 0,
        Success = 1,
        Failed = 2,
    }

    public interface IClientPurchasingService : IInjectable
    {
        Awaitable InitializeStores(CancellationToken token);
        Awaitable PurchaseBundle(PlayerStoreOffer offer, PlayerBundle bundle, CancellationToken token);
        EPurchasePlatforms GetPurchasePlatform();
        Task RetryPurchaseAfterLogin(CancellationToken token);

    }


    public class PurchaseResult
    {
        public string FailureReason { get; set; }
        public string ReceiptData { get; set; }
        public PendingOrder Pending { get; set; }
    }



    public class ClientProduct
    {

        public ClientProduct(Product product, ProductSku sku, string platformName)
        {
            Product = product;
            Sku = sku;

            if (platformName == ClientPlatformNames.iOS || platformName == ClientPlatformNames.Android)
            {
                LocalizedPriceString = product.metadata.localizedPriceString;
                LocalizedDescription = product.metadata.localizedDescription;
                LocalizedName = product.metadata.localizedTitle;
                LocalizedPrice = product.metadata.localizedPrice;
            }
            else
            {
                LocalizedPriceString = sku.DollarPrice.ToString();
                LocalizedDescription = sku.Desc;
                LocalizedName = sku.Name;
                LocalizedPrice = (decimal)sku.DollarPrice;
            }
        }


        public Product Product { get; set; }
        public ProductSku Sku { get; set; }


        public string LocalizedPriceString { get; private set; }
        public decimal LocalizedPrice { get; private set; }
        public string LocalizedDescription { get; private set; }
        public string LocalizedName { get; private set; }
    }

    public class ClientPurchasingService : IClientPurchasingService
    {

        private StoreController _storeController = null;
        private IClientGameState _gs = null;
        private IGameData _gameData = null;
        private IClientConfigContainer _configContainer = null;
        private IClientAppService _appService = null;
        private ILogService _logService = null;
        private IClientWebService _webService = null;
        private IRepositoryService _repoService = null;

        private EPurchasingInitializationState _state = EPurchasingInitializationState.NotInitialized;

        private Dictionary<string, ClientProduct> _productsByProductId = new Dictionary<string, ClientProduct>();
        private Dictionary<long, ClientProduct> _productsBySkuIdkey = new Dictionary<long, ClientProduct>();

        public async Awaitable InitializeStores(CancellationToken token)
        {
            _storeController = UnityIAPServices.StoreController();

            _storeController.OnPurchasePending += OnPending;

            await _storeController.Connect();

            IReadOnlyList<ProductSku> skus = _gameData.Get<ProductSkuSettings>(_gs.ch).GetData();

            List<ProductDefinition> productsToFetch = new List<ProductDefinition>();

            bool isApple = _appService.GetPlatformName() == ClientPlatformNames.iOS;

            foreach (ProductSku sku in skus)
            {
                productsToFetch.Add(new ProductDefinition((isApple ? sku.AppleProductId : sku.GoogleProductId), ProductType.Consumable));

            }

            _storeController.OnPurchasesFetched += OnPurchasesFetched;
            _storeController.OnProductsFetched += OnProductsFetched;
            _storeController.OnProductsFetchFailed += OnProductsFetchFailed;
            _storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            _storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            _storeController.OnPurchaseFailed += OnPurchaseFailed;
            _storeController.OnPurchaseDeferred += OnPurchaseDeferred;
            _storeController.OnPurchasePending += OnPurchasePending;

            _storeController.FetchProducts(productsToFetch);

            string env = _configContainer.Config.Env;

            InitializationOptions options = new InitializationOptions().SetEnvironmentName(env);

            await UnityServices.InitializeAsync(options);

            while (_state == EPurchasingInitializationState.NotInitialized)
            {
                await Awaitable.NextFrameAsync(token);
            }


            _logService.Info("Purchasing Initialization state: " + _state.ToString());

            await Task.CompletedTask;
        }

        private void OnPurchaseConfirmed(Order order)
        {
            _logService.Info("Purchase Confirmed!");
        }

        private void OnPurchaseFailed(FailedOrder failed)
        {
            _logService.Info("Purchase Failed!");
            _purchaseResult = new PurchaseResult()
            {
                FailureReason = failed.FailureReason.ToString() + " " + failed.Details
            };
        }

        private void OnPurchaseDeferred(DeferredOrder deferred)
        {
            _logService.Info("Purchase Deferred!");
            _purchaseResult = new PurchaseResult()
            {
                FailureReason = "Deferred",
            };
        }

        private void OnPurchasePending(PendingOrder pending)
        {
            _logService.Info("Purchase Pending!");
            _purchaseResult = new PurchaseResult()
            {
                FailureReason = null,
                Pending = pending,
                ReceiptData = pending.Info.Receipt,
            };
        }


        private async Task<CurrentPurchaseData> LoadCurrentPurchaseData()
        {
            CurrentPurchaseData data = await _repoService.Load<CurrentPurchaseData>(_gs.acct.Id);

            if (data == null)
            {
                data = new CurrentPurchaseData() { Id = _gs.acct.Id };
            }
            return data;
        }

        private async Task SaveCurrentPurchaseData(CurrentPurchaseData data)
        {
            await _repoService.Save(data);
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription obj)
        {
            _logService.Info("Fetch Purchases failed: " + obj.Message);
        }

        private void OnProductsFetchFailed(ProductFetchFailed obj)
        {
            _state = EPurchasingInitializationState.Failed;
            _logService.Info("Fetch Products Failed: " + obj.FailureReason);
        }


        private void OnPending(PendingOrder pendingOrder)
        {
            _logService.Info("On Pending Order: " + pendingOrder.Info.Receipt);
        }

        void OnProductsFetched(List<Product> products)
        {

            _productsByProductId = new Dictionary<string, ClientProduct>();

            IReadOnlyList<ProductSku> skus = _gameData.Get<ProductSkuSettings>(_gs.ch).GetData();

            string platformName = _appService.GetPlatformName();

            bool isApple = _appService.GetPlatformName() == ClientPlatformNames.iOS;

            foreach (Product prod in products)
            {

                ProductSku sku = skus.FirstOrDefault(x => prod.definition.id == (isApple ? x.AppleProductId : x.GoogleProductId));

                if (sku == null)
                {
                    _logService.Info("Bad Product sent to client: " + prod.definition.id);
                    continue;
                }

                ClientProduct newProduct = new ClientProduct(prod, sku, platformName);
                _productsByProductId[prod.definition.id] = newProduct;
                _productsBySkuIdkey[sku.IdKey] = newProduct;
            }

            _state = EPurchasingInitializationState.Success;
            _logService.Info("Fetch Products Success");



            // Handle fetched products  
            _storeController.FetchPurchases();
        }

        void OnPurchasesFetched(Orders orders)
        {
            _logService.Info("Fetch Purchases Succeeded");
        }

        public EPurchasePlatforms GetPurchasePlatform()
        {
            string platformName = _appService.GetPlatformName();

            if (platformName == ClientPlatformNames.iOS)
            {
                return EPurchasePlatforms.IOS;
            }
            else if (platformName == ClientPlatformNames.Android)
            {
                return EPurchasePlatforms.GooglePlay;
            }

            return EPurchasePlatforms.Editor;
        }

        public async Awaitable PurchaseBundle(PlayerStoreOffer offer, PlayerBundle bundle, CancellationToken token)
        {

            if (!_productsBySkuIdkey.TryGetValue(bundle.ProductSkuId, out ClientProduct clientProduct))
            {
                ShowErrorResponse("That bundle has an invalid product Id. Please try again later.");
                return;
            }

            InitiatePurchaseRequest initializeRequest = new InitiatePurchaseRequest()
            {
                OfferId = offer.OfferId,
                BundleId = bundle.BundleId,
                UniqueId = bundle.UniqueId,
                Platform = GetPurchasePlatform(),
            };

            InitiatePurchaseResponse initiateResponse = await _webService.SendClientUserWebRequestAsync<InitiatePurchaseResponse>(initializeRequest, token);

            if (initiateResponse == null)
            {
                await ClearLocalPurchaseData();
                ShowErrorResponse("No initialization response");
                return;
            }

            _logService.Info("State: " + initiateResponse.State.ToString());

            CurrentPurchaseData purchaseData = await LoadCurrentPurchaseData();

            if (initiateResponse.State != EInitiatePurchaseStates.Success)
            {
                await ClearLocalPurchaseData();
                ShowErrorResponse("Purchase Initialization failed: " + initiateResponse.State);
                return;
            }

            if (initiateResponse.ProductId != clientProduct.Product.definition.id)
            {
                purchaseData = new CurrentPurchaseData() { Id = _gs.acct.Id };
                await SaveCurrentPurchaseData(purchaseData);
                ShowErrorResponse("Product Id does not match with the server.");
                return;
            }

            purchaseData.Clear();
            purchaseData.OfferId = initiateResponse.OfferId;
            purchaseData.BundleId = initiateResponse.BundleId;
            purchaseData.UniqueId = initiateResponse.UniqueId;
            purchaseData.ReceiptData = null;
            purchaseData.ProductId = clientProduct.Product.definition.id;
            purchaseData.State = ECurrentPurchaseStates.Initiated;

            await SaveCurrentPurchaseData(purchaseData);


            await LocalPurchaseValidation(purchaseData, clientProduct, token);
            // Do local platform validation.

            if (_purchaseResult == null || _purchaseResult.Pending == null)
            {
                await ClearLocalPurchaseData(purchaseData);
                return;
            }

            if (!string.IsNullOrEmpty(_purchaseResult.FailureReason))
            {
                ShowErrorResponse(_purchaseResult.FailureReason);
                await ClearLocalPurchaseData(purchaseData);
                return;
            }

            string receiptData = _purchaseResult.ReceiptData;
            purchaseData.ReceiptData = _purchaseResult.ReceiptData;
            purchaseData.State = ECurrentPurchaseStates.ClientValidated;

            await SaveCurrentPurchaseData(purchaseData);

            await ServerValidatePurchase(purchaseData.OfferId, purchaseData.BundleId, purchaseData.UniqueId, purchaseData.Platform, receiptData, token);

            ConfirmPendingPurchase(_purchaseResult.Pending);
        }

        private void ConfirmPendingPurchase(PendingOrder pending)
        {
            _storeController.ConfirmPurchase(pending);
        }


        private PurchaseResult _purchaseResult = null;
        private async Awaitable LocalPurchaseValidation(CurrentPurchaseData purchaseData, ClientProduct clientProduct, CancellationToken token)
        {

            _purchaseResult = null;

            if (!clientProduct.Product.availableToPurchase)
            {
                ShowErrorResponse("That product is not currently available.");
                return;
            }

            _storeController.PurchaseProduct(clientProduct.Product);

            while (_purchaseResult == null)
            {
                await Awaitable.NextFrameAsync(token);
            }
        }


        private async Task ServerValidatePurchase(string offerId, string bundleId, string uniqueId, EPurchasePlatforms platform, string receiptData, CancellationToken token)
        {

            ValidatePurchaseRequest request = new ValidatePurchaseRequest()
            {
                OfferId = offerId,
                BundleId = bundleId,
                UniqueId = uniqueId,
                Platform = platform,
                ReceiptData = receiptData,
            };

            ValidatePurchaseResponse response = await _webService.SendClientUserWebRequestAsync<ValidatePurchaseResponse>(request, token);

            if (response == null)
            {
                _logService.Error("Missing validation response!");
                return;
            }

            // Did get a response from the server so we can delete the local validation data.
            if (response.State != EPurchaseValidationStates.Success)
            {
                await ClearLocalPurchaseData();
                _logService.Error("Failed validation: " + response.State);
                return;
            }
            else
            {
                await ClearLocalPurchaseData();
                _logService.Error("Success!");
            }

            await Task.CompletedTask;
        }

        private void ShowErrorResponse(string txt)
        {
            _logService.Info(txt);
        }

        private async Task ClearLocalPurchaseData(CurrentPurchaseData currentPurchaseData = null)
        {
            if (currentPurchaseData == null)
            {
                currentPurchaseData = await LoadCurrentPurchaseData();
            }
            currentPurchaseData.Clear();
            await SaveCurrentPurchaseData(currentPurchaseData);
        }

        public async Task RetryPurchaseAfterLogin(CancellationToken token)
        {
            CurrentPurchaseData purchaseData = await LoadCurrentPurchaseData();

            if (purchaseData.HasFullOrder() && purchaseData.FailedValidationTimes < 3)
            {
                purchaseData.FailedValidationTimes++;
                await SaveCurrentPurchaseData(purchaseData);
                await ServerValidatePurchase(purchaseData.OfferId, purchaseData.BundleId, purchaseData.UniqueId, purchaseData.Platform, purchaseData.ReceiptData, token);
            }
        }
    }
}

using MongoDB.Driver;
using OxDb.RequestServer.Core;
using OxDb.RequestServer.Purchasing.Entities;
using OxDb.RequestServer.Purchasing.ValidationHelpers;
using OxDb.ServerCore.Crypto.Services;
using OxDb.ServerCore.DataStores.Services;
using OxDb.ServerCore.GameSettings.Services;
using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Purchasing.Constants;
using OxDb.SharedGame.Purchasing.PlayerData;
using OxDb.SharedGame.Purchasing.Services;
using OxDb.SharedGame.Purchasing.Settings;
using OxDb.SharedGame.Purchasing.WebApi.InitializePurchase;
using OxDb.SharedGame.Purchasing.WebApi.ValidatePurchase;
using OxDb.SharedGame.Time.Services;
using OxDb.SharedGame.Versions.Settings;

namespace OxDb.RequestServer.Purchasing.Services
{
    public interface IServerPurchasingService : IInitializable
    {
        Task<PlayerStoreOfferData> GetCurrentStores(WebContext context, IFilteredObject user, bool forceRefresh, CancellationToken token);
        Task InitiatePurchase(WebContext context, InitiatePurchaseRequest request, CancellationToken token);
        Task ValidatePurchase(WebContext context, ValidatePurchaseRequest request, CancellationToken token);
        Task RetryFailedValidation(WebContext context, CancellationToken token);

    }
    public class ServerPurchasingService : IServerPurchasingService
    {
        protected IFullRepositoryService _repoService = null;
        private IGameData _gameData = null;
        private IServerGameDataService _gameDataService = null;
        private ICryptoService _cryptoService = null;
        private ITimeService _timeService = null;
        private IPurchasingService _purchasingService = null;

        private SetupDictionaryContainer<EPurchasePlatforms, IPurchaseValidationHelper> _validationHelpers = new SetupDictionaryContainer<EPurchasePlatforms, IPurchaseValidationHelper>();

        // Try to connect to apple and google?
        public async Task Initialize(CancellationToken token)
        {

            CreateIndexData data = new CreateIndexData(typeof(CompletedPurchaseData));
            data.Configs.Add(new IndexConfig() { MemberName = nameof(CompletedPurchaseData.ReceiptHash) });
            await _repoService.CreateIndexes(data);
        }

        #region GetStores
        public async Task<PlayerStoreOfferData> GetCurrentStores(WebContext context, IFilteredObject user, bool forceRefresh, CancellationToken token)
        {

            PlayerStoreOfferData storeOfferData = await context.GetAsync<PlayerStoreOfferData>();

            if (storeOfferData == null)
            {
                storeOfferData = new PlayerStoreOfferData() { Id = user.Id };
            }

            DateTime currentTime = _timeService.GetTime(user);

            VersionSettings versionSettings = _gameData.Get<VersionSettings>(user);

            StoreOfferSettings storeOfferSettings = _gameData.Get<StoreOfferSettings>(user);
            StoreFeatureSettings featureSettings = _gameData.Get<StoreFeatureSettings>(user);
            StoreSlotSettings slotSettings = _gameData.Get<StoreSlotSettings>(user);

            if (storeOfferSettings.GetNextUpdateTime(currentTime) <= currentTime)
            {
                storeOfferSettings.SetPrevNextUpdateTimes(currentTime);
            }

            if (!forceRefresh &&
            versionSettings.SaveTime == storeOfferData.GameDataSaveTime &&
            storeOfferData.LastTimeSet >= storeOfferSettings.GetPrevUpdateTime(currentTime) &&
            storeOfferData.LastTimeSet < storeOfferSettings.GetNextUpdateTime(currentTime))
            {
                // stores are the same
                return storeOfferData;
            }

            IReadOnlyList<StoreOffer> storeOffers = _gameData.Get<StoreOfferSettings>(user).GetData();

            Dictionary<long, StoreOffer> storeDict = new Dictionary<long, StoreOffer>();

            storeOfferData.StoreOffers.Clear();

            PurchaseHistoryData historyData = await context.GetAsync<PurchaseHistoryData>();

            foreach (StoreOffer offer in storeOffers)
            {
                TryAddOffer(offer, currentTime, storeDict, user, historyData);
            }

            foreach (StoreOffer storeOffer in storeDict.Values)
            {

                PlayerStoreOffer playerStoreOffer = _purchasingService.CreatePlayerStoreOffer(user, storeOffer);

                if (playerStoreOffer != null)
                {
                    storeOfferData.StoreOffers.Add(playerStoreOffer);
                }
            }

            storeOfferData.GameDataSaveTime = versionSettings.SaveTime;
            storeOfferData.LastTimeSet = currentTime;

            return storeOfferData;
        }


        protected void TryAddOffer(StoreOffer offer, DateTime currentTime, Dictionary<long, StoreOffer> currentOffers, IFilteredObject user, PurchaseHistoryData historyData)
        {

            if (!_gameDataService.AcceptedByFilter(user, offer, currentTime))
            {
                return;
            }

            if (currentOffers.TryGetValue(offer.StoreSlotId, out StoreOffer currentOffer) &&
                currentOffer.Priority >= offer.Priority)
            {
                return;
            }

            bool forceAddThroughId = false;

            if (offer.IsDefaultOffer)
            {
                return;
            }

            if (offer.AllowedPlayers.Count > 0)
            {
                if (offer.AllowedPlayers.Any(x => x.PlayerId == user.Id))
                {
                    forceAddThroughId = true;
                }
            }

            if (!forceAddThroughId)
            {

                if (offer.MinPurchaseCount > 0 && historyData.PurchaseCount < offer.MinPurchaseCount)
                {
                    return;
                }

                if (offer.MaxPurchaseCount > 0 && historyData.PurchaseCount < offer.MaxPurchaseCount)
                {
                    return;
                }

                if (offer.MinPurchaseTotal > 0 && historyData.PurchaseTotal < offer.MinPurchaseTotal)
                {
                    return;
                }

                if (offer.MaxPurchaseTotal > 0 && historyData.PurchaseTotal > offer.MaxPurchaseTotal)
                {
                    return;
                }

            }

            currentOffers[offer.StoreSlotId] = offer;
        }

        #endregion

        #region InitiatePurchase


        public async Task InitiatePurchase(WebContext context, InitiatePurchaseRequest request, CancellationToken token)
        {

            CurrentPurchaseData purchaseData = await context.GetAsync<CurrentPurchaseData>();

            if (purchaseData != null && purchaseData.State != ECurrentPurchaseStates.None)
            {
                // Current receipt.
            }

            CoreData coreData = await context.GetAsync<CoreData>();
            DefaultStoreOfferSettings defaultOffers = _gameData.Get<DefaultStoreOfferSettings>(coreData);
            PlayerStoreOfferData offerData = await context.GetAsync<PlayerStoreOfferData>();

            PlayerStoreOffer playerStoreOffer = offerData.StoreOffers.FirstOrDefault(x => x.OfferId == request.OfferId);

            if (playerStoreOffer == null)
            {
                playerStoreOffer = defaultOffers.Offers.FirstOrDefault(x => x.OfferId == request.OfferId);

                if (playerStoreOffer == null)
                {
                    CreatePurchaseIntentErrorResponse(context, request, EInitiatePurchaseStates.MissingPlayerStoreOffer);
                    return;
                }
            }

            PlayerBundle playerBundle = playerStoreOffer.Bundles.FirstOrDefault(x => x.BundleId == request.BundleId);

            if (playerBundle == null || playerBundle.UniqueId != request.UniqueId)
            {
                CreatePurchaseIntentErrorResponse(context, request, EInitiatePurchaseStates.MissingPlayerBundle);
                return;
            }

            if (playerBundle.Rewards == null || playerBundle.Rewards.Count < 1)
            {
                CreatePurchaseIntentErrorResponse(context, request, EInitiatePurchaseStates.MissingPlayerStoreItem);
                return;
            }

            ProductSku currentSku = _gameData.Get<ProductSkuSettings>(coreData).Get(playerBundle.ProductSkuId);

            if (currentSku == null)
            {
                CreatePurchaseIntentErrorResponse(context, request, EInitiatePurchaseStates.MissingGameDataSku);
                return;
            }

            purchaseData.OfferId = request.OfferId;
            purchaseData.BundleId = request.BundleId;
            purchaseData.UniqueId = request.UniqueId;
            purchaseData.Platform = request.Platform;
            purchaseData.ProductId = GetProductIdFromPlatform(currentSku, request.Platform);
            purchaseData.State = ECurrentPurchaseStates.Initiated;
            purchaseData.Rewards = playerBundle.Rewards.ToList();
            purchaseData.ReceiptData = null;

            CreatePurchaseIntentSuccessResponse(context, purchaseData);

            await Task.CompletedTask;
        }

        private void CreatePurchaseIntentSuccessResponse(WebContext context, CurrentPurchaseData purchaseData)
        {
            context.AddResponse(new InitiatePurchaseResponse()
            {
                State = EInitiatePurchaseStates.Success,
                OfferId = purchaseData.OfferId,
                BundleId = purchaseData.BundleId,
                UniqueId = purchaseData.UniqueId,
                ProductId = purchaseData.ProductId,

            });
        }

        private void CreatePurchaseIntentErrorResponse(WebContext context, InitiatePurchaseRequest request, EInitiatePurchaseStates response)
        {
            context.AddResponse(new InitiatePurchaseResponse()
            {
                State = response,
                UniqueId = request.UniqueId,
                BundleId = request.BundleId,
                OfferId = request.OfferId,
            });
        }

        #endregion


        #region ValidatePurchase


        public async Task RetryFailedValidation(WebContext context, CancellationToken token)
        {
            CurrentPurchaseData purchaseData = await context.GetAsync<CurrentPurchaseData>();

            if ((purchaseData.State == ECurrentPurchaseStates.FailedValidation || purchaseData.State == ECurrentPurchaseStates.Initiated) && purchaseData.FailedValidationTimes < 3)
            {
                if (string.IsNullOrEmpty(purchaseData.ReceiptData) || string.IsNullOrEmpty(purchaseData.ProductId) || purchaseData.Rewards == null || purchaseData.Rewards.Count < 1)
                {
                    // Bad data, do not retry.
                    purchaseData.Clear();
                    return;
                }

                if (purchaseData.HasFullOrder() && purchaseData.Rewards != null && purchaseData.Rewards.Count > 0)
                {
                    await ValidatePurchaseInternal(context, purchaseData.OfferId, purchaseData.BundleId, purchaseData.UniqueId, purchaseData.ProductId, purchaseData.ReceiptData, purchaseData.Platform, purchaseData.Rewards, token);
                }
            }
        }


        private void CreateValidationErrorResponse(WebContext context, CurrentPurchaseData purchaseData, EPurchaseValidationStates state, string errorMessage = null)
        {
            purchaseData.State = ECurrentPurchaseStates.FailedValidation;
            purchaseData.FailedValidationTimes++;

            context.AddResponse(new ValidatePurchaseResponse() { ErrorMessage = errorMessage, State = state });
        }

        private void CreateValidationSuccessResponse(WebContext context, CurrentPurchaseData purchaseData)
        {
            purchaseData.State = ECurrentPurchaseStates.Validated;
            purchaseData.FailedValidationTimes = 0;
            context.AddResponse(new ValidatePurchaseResponse() { State = EPurchaseValidationStates.Success });
        }
        private bool AllDataIsOk(string offerId, string bundleId, string uniqueId, string productId)
        {
            return !string.IsNullOrEmpty(offerId) &&
                !string.IsNullOrEmpty(bundleId) &&
                !string.IsNullOrEmpty(uniqueId) &&
                !string.IsNullOrEmpty(productId);
        }

        private string GetProductIdFromPlatform(ProductSku sku, EPurchasePlatforms platform)
        {
            if (platform == EPurchasePlatforms.IOS)
            {
                return sku.AppleProductId;
            }
            return sku.GoogleProductId;
        }

        public async Task ValidatePurchase(WebContext context, ValidatePurchaseRequest request, CancellationToken token)
        {
            CurrentPurchaseData currentPurchase = await context.GetAsync<CurrentPurchaseData>();

            if (string.IsNullOrEmpty(request.ReceiptData))
            {
                CreateValidationErrorResponse(context, currentPurchase, EPurchaseValidationStates.NoReceipt);
                return;
            }

            if (string.IsNullOrEmpty(request.OfferId))
            {
                CreateValidationErrorResponse(context, currentPurchase, EPurchaseValidationStates.MissingOfferId);
                return;
            }

            if (string.IsNullOrEmpty(request.BundleId))
            {
                CreateValidationErrorResponse(context, currentPurchase, EPurchaseValidationStates.MissingBundleId);
                return;
            }

            if (string.IsNullOrEmpty(request.UniqueId))
            {
                CreateValidationErrorResponse(context, currentPurchase, EPurchaseValidationStates.MissingUniqueId);
                return;
            }

            CoreData coreData = await context.GetAsync<CoreData>();

            string offerId = null;
            string bundleId = null;
            string uniqueId = null;
            string productId = null;
            string receiptData = request.ReceiptData;
            EPurchasePlatforms platform = request.Platform;
            List<Reward> rewards = new List<Reward>();

            if (currentPurchase != null &&
                currentPurchase.OfferId == request.OfferId &&
                currentPurchase.BundleId == request.BundleId &&
                currentPurchase.UniqueId == request.UniqueId &&
                currentPurchase.Platform == request.Platform &&
                currentPurchase.Rewards != null &&
                currentPurchase.Rewards.Count > 0 &&
                !string.IsNullOrEmpty(currentPurchase.ProductId))
            {
                offerId = request.OfferId;
                bundleId = request.BundleId;
                uniqueId = request.UniqueId;
                rewards = currentPurchase.Rewards;
                platform = currentPurchase.Platform;
                productId = currentPurchase.ProductId;
            }

            if (!AllDataIsOk(offerId, bundleId, uniqueId, productId))
            {
                StoreOfferSettings storeOfferSettings = _gameData.Get<StoreOfferSettings>(coreData);

                StoreOffer offer = storeOfferSettings.GetData().FirstOrDefault(x => x.OfferId == request.OfferId);

                if (offer != null)
                {
                    StoreBundleSet bundleSet = _gameData.Get<StoreBundleSetSettings>(coreData).Get(offer.StoreBundleSetId);

                    if (bundleSet != null)
                    {
                        StoreBundle bundle = bundleSet.Bundles.FirstOrDefault(x => x.BundleId == request.BundleId);

                        if (bundle != null)
                        {
                            ProductSku sku = _gameData.Get<ProductSkuSettings>(coreData).Get(bundle.ProductSkuId);

                            if (sku != null)
                            {
                                productId = GetProductIdFromPlatform(sku, request.Platform);

                                if (bundle.Rewards != null && bundle.Rewards.Count > 0)
                                {
                                    offerId = offer.OfferId;
                                    bundleId = bundle.BundleId;
                                    uniqueId = "Recovery";
                                    rewards = bundle.Rewards.ToList();
                                }
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(offerId))
            {
                CreateValidationErrorResponse(context, currentPurchase, EPurchaseValidationStates.MissingOfferId, "Missing Store Offer");
                return;
            }
            if (string.IsNullOrEmpty(bundleId))
            {
                CreateValidationErrorResponse(context, currentPurchase, EPurchaseValidationStates.MissingBundleId, "Missing Bundle");
                return;
            }

            if (string.IsNullOrEmpty(uniqueId))
            {
                CreateValidationErrorResponse(context, currentPurchase, EPurchaseValidationStates.MissingUniqueId, "Missing Unique Id");
                return;
            }

            if (string.IsNullOrEmpty(productId))
            {
                CreateValidationErrorResponse(context, currentPurchase, EPurchaseValidationStates.MissingProductId, "Missng Product Id");
                return;
            }

            await ValidatePurchaseInternal(context, offerId, bundleId, uniqueId, productId, receiptData, platform, rewards, token);

        }


        private async Task ValidatePurchaseInternal(WebContext context, string offerId, string bundleId, string uniqueId, string productId, string receiptData, EPurchasePlatforms platform,
            List<Reward> rewards, CancellationToken token)
        {
            string hashedReceipt = _cryptoService.SlowHash(receiptData);

            CurrentPurchaseData currentPurchase = await context.GetAsync<CurrentPurchaseData>();

            List<CompletedPurchaseData> allCompleted = await _repoService.Search<CompletedPurchaseData>(x => x.ReceiptHash == hashedReceipt);

            if (allCompleted.Any(x => x.ReceiptData == receiptData))
            {
                CreateValidationErrorResponse(context, currentPurchase, EPurchaseValidationStates.AlreadyValidated, "This receipt has been validated.");
                return;
            }

            PurchaseValidationResult result = null;

            if (_validationHelpers.TryGetValue(platform, out IPurchaseValidationHelper helper))
            {
                result = await helper.ValidatePurchase(productId, receiptData);
            }
            else
            {
                CreateValidationErrorResponse(context, currentPurchase, EPurchaseValidationStates.InvalidPlatform);
                return;
            }

            if (result.State != EPurchaseValidationStates.Success)
            {
                CreateValidationErrorResponse(context, currentPurchase, result.State, result.ErrorMessage);
                return;
            }

            await GiveRewards(context, rewards, token);

            currentPurchase.State = ECurrentPurchaseStates.Validated;

            CreateValidationSuccessResponse(context, currentPurchase);

            currentPurchase.Clear();

            CoreData coreData = await context.GetAsync<CoreData>();

            await GetCurrentStores(context, coreData, true, token);
            await Task.CompletedTask;
        }

        private async Task GiveRewards(WebContext context, List<Reward> rewards, CancellationToken token)
        {

            await Task.CompletedTask;
        }

        #endregion
    }
}



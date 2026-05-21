namespace OxDb.SharedGame.Purchasing.Constants
{
    public enum EInitiatePurchaseStates
    {
        Failed = 0,
        Success = 1,
        MissingStoreOffer = 2,
        MissingOfferProduct = 3,
        MissingPlayerStoreOffer = 4,
        OfferIsAlreadyInitialized = 5,
        MissingPlayerBundle = 6,
        MissingOfferItemSku = 7,
        MissingGameDataSku = 8,
        MissingPlayerStoreItem = 9,
    }
}



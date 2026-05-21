namespace OxDb.SharedGame.Purchasing.Constants
{
    public enum EPurchaseValidationStates
    {
        Failed = 0,
        Success = 1,
        MissingOfferId = 2,
        MissingBundleId = 3,
        AlreadyValidated = 4,
        MissingUniqueId = 6,
        MissingProductId = 7,
        InvalidPlatform = 10,
        NoReceipt = 11,
    }
}



namespace OxDb.ServerCore.Platform.Constants
{
    public enum EPlatformAuthStates
    {
        Success = 0,
        AccountDoesNotExist = 1,
        ProductWasNotAdded = 2,
        IncorrectSessionId = 3,
        ProductAccountWasAlreadyCreated = 4,
        ProductUserIdDoesNotMatch = 5,
        ExistingGameDataIsMissing = 6,
    };
}

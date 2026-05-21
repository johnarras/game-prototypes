namespace OxDb.RequestServer.GameAuthRequests.Constants
{
    public enum EGameAuthStates
    {
        Success = 0,
        MissingExistingAccount = 1,
        IncorrectGameUserId = 2,
        NoUserWithThatId = 3,
        FailedToPersistData = 4,
    }
}

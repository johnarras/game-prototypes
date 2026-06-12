namespace OxDb.PlatformServer.Accounts.PlayerData
{
    public class AccountSessionData : BaseAccountData
    {
        public override string Id { get; set; }
        public string AccountSessionId { get; set; }
        public string DisplayName { get; set; }
    }
}



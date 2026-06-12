using OxDb.SharedCore.Website.Responses.Interfaces;

namespace OxDb.SharedPlatform.Accounts.WebApi.AccountAuth
{
    public class AccountAuthResponse : IWebResponse
    {
        public string AccountId { get; set; }
        public string LoginToken { get; set; }
        public string DeviceId { get; set; }
        public string AccountSessionId { get; set; }
        public string ProductUserId { get; set; }
        public long DataBits { get; set; }
        public long ProductId { get; set; }
        public string DisplayName { get; set; }
        public string InstallSource { get; set; }
    }
}

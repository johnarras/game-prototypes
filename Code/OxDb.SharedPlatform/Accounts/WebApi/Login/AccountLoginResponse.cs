using OxDb.SharedCore.Website.Responses.Interfaces;

namespace OxDb.SharedPlatform.Accounts.WebApi.Login
{
    public class AccountLoginResponse : IWebResponse
    {
        public string AccountId { get; set; }
        public string LoginToken { get; set; }
        public string SessionId { get; set; }
        public string ProductUserId { get; set; } // This CAN be different in the future.
        public long DataBits { get; set; }
        public long ProductId { get; set; }
        public string ShareId { get; set; }
    }
}



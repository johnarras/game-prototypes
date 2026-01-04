using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Accounts.WebApi.Login
{
    public class AccountLoginResponse : IWebResponse
    {
        public string AccountId { get; set; }
        public string LoginToken { get; set; }
        public string SessionId { get; set; }
        public string GameUserId { get; set; } // This CAN be different in the future.
    }
}



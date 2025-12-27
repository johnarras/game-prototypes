using Genrpg.Shared.Website.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Accounts.WebApi.Login
{
    public class AccountLoginResponse : IWebResponse
    {
        public string AccountId { get; set; }
        public string LoginToken { get; set; }
        public string SessionId { get; set; }
    }
}



using Genrpg.Shared.Website.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Accounts.WebApi.Login
{
    [MessagePackObject]
    public class AccountLoginResponse : IWebResponse
    {
        [Key(0)] public string AccountId { get; set; }
        [Key(1)] public string LoginToken { get; set; }
        [Key(2)] public string SessionId { get; set; }
    }
}

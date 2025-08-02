using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Accounts.WebApi.Login
{
    [MessagePackObject]
    public class AccountLoginResponse : IWebResponse
    {
        [Key(0)] public string AccountId { get; set; }
        [Key(1)] public string ProductAccountId { get; set; }
        [Key(2)] public string LoginToken { get; set; }
        [Key(3)] public string SessionId { get; set; }
    }
}

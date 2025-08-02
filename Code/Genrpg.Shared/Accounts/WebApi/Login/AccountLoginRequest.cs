using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.Website.Interfaces;
using System;

namespace Genrpg.Shared.Accounts.WebApi.Login
{
    [MessagePackObject]
    public class AccountLoginRequest : IAccountAuthRequest
    {
        [Key(0)] public string UserId { get; set; }
        [Key(1)] public string Email { get; set; }
        [Key(2)] public string Password { get; set; }

        [Key(3)] public long AccountProductId { get; set; }

        [Key(4)] public string ReferrerId { get; set; }

        [Key(5)] public string DeviceId { get; set; }


    }
}

using OxDb.SharedCore.Website.Interfaces;
using System;

namespace OxDb.SharedPlatform.Accounts.WebApi.Signup
{
    public class AccountSignupRequest : IAccountAuthRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string ShareId { get; set; }
        public string ReferrerId { get; set; }
        public long ProductId { get; set; }
        public string ClientVersion { get; set; }

        public string DeviceId { get; set; }

        public DateTime ClientGameDataSaveTime { get; set; }
    }
}


